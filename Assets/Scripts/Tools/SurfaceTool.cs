using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;
using System.Xml.Serialization;
using SharpKml;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System.Globalization;
using System.Text.RegularExpressions;

public class SurfaceTool : Tool
{

    public static int localID = 0;
    [HideInInspector]
    public static List<GameObject> instanceList = new List<GameObject>();
    [HideInInspector]
    public static List<string> surfaceMap = new List<string>();

    private string _note = "";
    string oldNote = "";

    public override IEnumerator ToolCoroutine()
    {
        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();
        ToolController.ToolIsCurrentlyRunning = true;
        ToolController.ToolControllerInterfaceIsCurrentlyRunning = false;

        toolControllerComponent.MeasurementControlUI.gameObject.SetActive(false);
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("ToolMenu").gameObject.transform.Find("Group2").gameObject.transform.Find("Field_notebook").gameObject.GetComponent<FieldNotes>().outputString = "";
        }
        // hold until trigger is released // this avoids instant placement 
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
        {
            while (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.2f)
            {
                yield return wfeof;
            }
        }

        // temporary objects
        GameObject TempPlacemark = null, TempPlacemarkInfo = null;
        Vector3 TempNormal = new Vector3();
        List<GameObject> placemarks = new List<GameObject>();
        List<Vector3> Normals = new List<Vector3>();
        Vector3Decimal realPosition;

        // Place markers
        for (int times = 0; times < 1; times++)
        {
            //Instantiate placemark
            TempPlacemark = Instantiate(toolControllerComponent.SurfaceToolObject);
            TempPlacemark.transform.localScale *= toolControllerComponent.MarkerScale;
            TempPlacemark.name = "distance placemark";

            //boolean flags
            bool placemarkConfirmed = false;
            bool legalPlacemarkPlace = false;
            bool wasHolding = true;

            //Handling Placemark
            RaycastHit hit;
            while (placemarkConfirmed == false)
            {
                // end coroutine early ?
                if (checkIfToolShouldQuit())
                {
                    toolControllerComponent.OculusMasterObject.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.2783019f, 0.3061422f, 1, 1);

                    placemarkConfirmed = true;
                    Destroy(TempPlacemark);
                    foreach (var p in placemarks)
                    {
                        Destroy(p);
                    }
                    ToolController.ToolIsCurrentlyRunning = false;
                    yield break;
                }

                // holding button?
                if (StateSingleton.stateView != StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView != StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
                {
                    if (wasHolding == true && OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) < 0.2f)
                        wasHolding = false;
                }

                // Trailing Waypoint
                if (Physics.Raycast(master.transform.position, directionMaster.transform.forward, out hit, 100000)) //  <--raycast for placement
                {
                    TempPlacemark.transform.position = hit.point;
                    TempNormal = hit.normal;

                    Vector3 _FlattenedNormal = Vector3.ProjectOnPlane(TempNormal, Vector3.up).normalized;
                    Vector3 _StrikeVector = Quaternion.Euler(0, -90, 0) * _FlattenedNormal; // Stride as Unity World Vector
                    Vector3 _DipVector = Vector3.Cross(TempNormal, _StrikeVector); // Slope as Unity World Vector

                    // convert vectors to angles 
                    float _DipDirection = VirtualMeter.CalculateHeading(_FlattenedNormal);
                    float _Strike = VirtualMeter.CalculateHeading(_StrikeVector);
                    float _DipAngle = VirtualMeter.CalculateInclination(_DipVector);

                    TempPlacemark.transform.eulerAngles = new Vector3(0.0f, _DipDirection, 0.0f);
                    TempPlacemark.transform.Find("Dip").gameObject.transform.localEulerAngles = new Vector3(90.0f - _DipAngle, 0.0f, 0.0f);

                    legalPlacemarkPlace = true;

                    //Showing info on toolInfo
                    realPosition = VirtualMeter.CalculateGPSPosition(TempPlacemark.transform.position);
                    toolControllerComponent.updateGUIMenus(
                      "Surface",
                      "Northing (m): " + realPosition.z.ToString("0.000") +
                      "\nEasting (m): " + realPosition.x.ToString("0.000") +
                      "\nAltitude (m): " + realPosition.y.ToString("0.000")
                      );

                }
                else  //If not well placed then invalidate
                {
                    TempPlacemark.transform.position = master.transform.position + directionMaster.transform.forward * 100000;
                    toolControllerComponent.updateGUIMenus(
                        "Placemark", "Northing (m): undefined \nEasting (m): undefined \nAltitude (m): undefined");
                    legalPlacemarkPlace = false;
                }

                // if triggered 
                if (
                    (Input.GetMouseButton(0) ||
                        (
                            (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS) 
                            && OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.5f && wasHolding == false )
                        )
                    && legalPlacemarkPlace
                    )
                {
                    toolControllerComponent.OculusMasterObject.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.2783019f, 0.3061422f, 1, 1);

                    placemarkConfirmed = true;
                    placemarks.Add(TempPlacemark);
                    Normals.Add(TempNormal);
                }
                yield return wfeof;
            }
            while (Input.GetMouseButton(0))
                yield return wfeof;
        }

        localID++;

        //do calculations        
        //https://en.wikipedia.org/wiki/Strike_and_dip
        //

        Vector3 FlattenedNormal = Vector3.ProjectOnPlane(Normals[0], Vector3.up).normalized;
        Vector3 StrikeVector = Quaternion.Euler(0, -90, 0) * FlattenedNormal; // Stride as Unity World Vector
        Vector3 DipVector = Vector3.Cross(Normals[0], StrikeVector); // Slope as Unity World Vector

        // convert vectors to angles 
       float DipDirection = VirtualMeter.CalculateHeading(Quaternion.Euler(0, 180, 0) * FlattenedNormal);
        float Strike       = VirtualMeter.CalculateHeading(StrikeVector);
        float DipAngle     = VirtualMeter.CalculateInclination(DipVector);

        Dictionary<string, decimal> Values = new Dictionary<string, decimal>{
            { "DipDirection" , (decimal)DipDirection },
            { "Strike" , (decimal)Strike },
            { "Dip" , (decimal)DipAngle }
        };


        oldNote = "Strike (°): " + Strike.ToString("0.000") +
            "\nDip Direction (°): " + DipDirection.ToString("0.000") +
            "\nDip (°): " + DipAngle.ToString("0.000") + "\nNote: ";

        DateTime date = DateTime.Now;
        string globalID = date.ToString("yyyy.MMddHmmssffff") + ",";

        // create tool instance        
        toolControllerComponent.CreateToolInstance("Surface",
            oldNote, "",
            Tool.toolType.SURFACE,
            Values,
            placemarks, date, true, false, true, localID, globalID);
    }

    public void startToolInterface()
    {
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("UpperPanel").gameObject.transform.Find("Text").gameObject.GetComponent<Text>().text = "Surface tool";

        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_line").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_surface").gameObject.SetActive(true);

     


        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_line").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_surface").gameObject.SetActive(true);


        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_line").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_surface").gameObject.SetActive(true);

        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_line").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_surface").gameObject.SetActive(true);

        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_line").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_surface").gameObject.SetActive(true);



        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_line").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_surface").gameObject.SetActive(true);


        GameObject.Find("Canvas").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.Find("Canvas").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.AddListener(GetInsertedNotes);


        toolControllerComponent.MeasurementControlUI.SetActive(true);
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("UpperPanel").gameObject.transform.Find("Text").gameObject.GetComponent<Text>().text = "Surface tool";
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.SetActive(true);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_ruler").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_surface").gameObject.SetActive(true);


            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_surface").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_ruler").gameObject.SetActive(true);


            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_surface").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_ruler").gameObject.SetActive(false);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_ruler").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_surface").gameObject.SetActive(true);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_ruler").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_surface").gameObject.SetActive(true);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_ruler").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_surface").gameObject.SetActive(true);


            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.AddListener(GetInsertedNotes);


            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").transform.localPosition = new Vector3(0.0f, -400.0f, 0.0f);

        }
        PauseAndGUIBehaviour.isToolMenu = false;
        ToolController.ToolControllerInterfaceIsCurrentlyRunning = true;
    }

    public void GetInsertedNotes()
    {
        _note = GameObject.Find("Canvas_Oculus").gameObject.transform.Find("ToolMenu").gameObject.transform.Find("Group2").gameObject.transform.Find("Field_notebook").gameObject.GetComponent<FieldNotes>().outputString;


        if (instanceList.Count != 0)
        {


            ToolInstance currentInstance = instanceList[instanceList.Count - 1].GetComponent<ToolInstance>();
            currentInstance.CustomTxt = _note;


            if (GameObject.Find(currentInstance.GUIMenu.name) != null)
                GameObject.Find(currentInstance.GUIMenu.name).gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("InputField").GetComponent<InputField>().text = oldNote + "" + currentInstance.CustomTxt;
            if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
                GameObject.Find(currentInstance.OculusMenu.name).gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("InputField").GetComponent<InputField>().text = oldNote + "" + currentInstance.CustomTxt;



        }
    }

    public void CancelButton()
    {
        toolControllerComponent.MeasurementControlUI.SetActive(false);

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.SetActive(false);
        }

        if (PauseAndGUIBehaviour.isPause)
            PauseAndGUIBehaviour.isPause = false;
        ToolController.ToolIsCurrentlyRunning = false;
        ToolController.ToolControllerInterfaceIsCurrentlyRunning = false;
    }

    public void ImportData()
    {

        string FilePath = "Import";

        var directory = new DirectoryInfo(Application.dataPath);
        var directoryPath = Path.Combine(directory.Parent.FullName, FilePath);

        var path = Path.Combine(directoryPath, string.Format("surfaces.csv"));
        
        LoadFromFile(path.ToString());
        ToolController.globalToolControllerObject.StartCoroutine(Tool.ShowNotificationLabelForMesuring("Done!", 1.5f));

    }


    public override GameObject LoadFromFile(string FilePath)
    {


        if (File.Exists(FilePath))
        {

            String fileData = System.IO.File.ReadAllText(FilePath);
            String[] lines = fileData.Split("\n"[0]);
            GameObject TempPlacemark;

            foreach (String line in lines)
            {
                Debug.Log("line: " + line);
                String[] data = line.Split(","[0]);

                
                if (data.Length == 17 && data[0][0].ToString() != "#") // discard non-coordinate lines
                {

                    if (!surfaceMap.Contains(data[1]))
                    {
                        List<GameObject> placemarks = new List<GameObject>();
                    

                        decimal z = decimal.Parse(data[8],  NumberStyles.Any, new CultureInfo("en-GB"));
                        decimal x = decimal.Parse(data[9],  NumberStyles.Any, new CultureInfo("en-GB"));
                        decimal y = decimal.Parse(data[10],  NumberStyles.Any, new CultureInfo("en-GB"));
                        
                        decimal rot_z = decimal.Parse(data[11],  NumberStyles.Any, new CultureInfo("en-GB"));

                        decimal rot_x = decimal.Parse(data[12], NumberStyles.Any, new CultureInfo("en-GB"));

                        decimal rot_y = decimal.Parse(data[13],  NumberStyles.Any, new CultureInfo("en-GB"));

                        decimal rot_w = decimal.Parse(data[14],  NumberStyles.Any, new CultureInfo("en-GB"));

                        string date = data[15];
                        string comment = Regex.Replace(data[16], @"\t|\n|\r", "");


                        //Instantiate placemark
                        TempPlacemark = Instantiate(toolControllerComponent.SurfaceToolObject);
                        TempPlacemark.transform.localScale *= toolControllerComponent.MarkerScale;
                        TempPlacemark.name = "distance placemark";


                        //TempPlacemark.transform.position = new Vector3((float)unityPosition.z, (float)unityPosition.x, (float)unityPosition.y); // this will need to account for offset
                        TempPlacemark.transform.position = new Vector3((float)x, (float)y, (float)z); // this will need to account for offset
                        TempPlacemark.transform.rotation = new Quaternion((float)rot_x, (float)rot_y, (float)rot_z, (float)rot_w); // this will need to account for offset
                        placemarks.Add(TempPlacemark);




                      

                        Dictionary<string, decimal> Values = new Dictionary<string, decimal>{
            { "DipDirection" , (decimal) decimal.Parse(data[7], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB"))},
            { "Strike" , (decimal)decimal.Parse(data[5], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB")) },
            { "Dip" , (decimal)decimal.Parse(data[6], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB")) }
        };


                        oldNote = "Strike (°): " + decimal.Parse(data[5], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB")).ToString("0.000") +
                            "\nDip Direction (°): " + decimal.Parse(data[7], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB")).ToString("0.000") +
                            "\nDip (°): " + decimal.Parse(data[6], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB")).ToString("0.000") + "\nNote: ";


                        DateTime myDate = DateTime.ParseExact(date, "MM/dd/yyyy H:mm:ss.ffff",
                                          System.Globalization.CultureInfo.InvariantCulture);

                        // create tool instance        
                        toolControllerComponent.CreateToolInstance("Surface",
                            oldNote, "",
                            Tool.toolType.SURFACE,
                            Values,
                            placemarks, myDate, true, false, false, localID, data[1], false);





                        localID++;
                    }
                    else
                    {
                        Debug.Log("elemento già presente");

                    }
                }
            }


        }
        return null;
    }
    // export

    public static void SaveSingleInstance(ToolInstance instance)
    {
        string FilePath = "Outputs/Surfaces";


        // CSV export

        var directory = new DirectoryInfo(Application.dataPath);
        var directoryPath = Path.Combine(directory.Parent.FullName, FilePath);

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
        }

        var path = Path.Combine(directoryPath, string.Format("{0}.csv", instance.ToolTitle.ToString() + " " + DateTime.Now.ToString("yyyyMMdd_Hmmssffff")));

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        //var sr = File.CreateText(path);
        StreamWriter writer = new StreamWriter(path, true);

        string csvData = "";
        csvData += "#Index, Id, Lat, Lon, z, Strike, Dip, DipDirection, unity_z, unity_x,unity_y, unity_rotation_z, unity_rotation_x, unity_rotation_y,unity_rotation_w, Date, Comments";
        writer.WriteLine(csvData, "en-GB");

        Vector3Decimal realPosition = new Vector3Decimal();
        for (int i = 0; i < instance.SurfaceList.Count; ++i)
        {
            realPosition = VirtualMeter.CalculateGPSPosition(instance.SurfaceList[i].transform.position);

            Debug.Log(realPosition.x);

            csvData = i.ToString() + ",";
            csvData += instance.ID;//creationDate.ToString("yyyy.MMddHmmssffff") + ",";
            csvData += realPosition.z.ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
            csvData += realPosition.x.ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
            csvData += realPosition.y.ToString("0.000", new CultureInfo("en-GB")) + ",";
            csvData += instance.ValueDict["Strike"].ToString("0.000", new CultureInfo("en-GB")) + ",";
            csvData += instance.ValueDict["Dip"].ToString("0.000", new CultureInfo("en-GB")) + ",";
            csvData += instance.ValueDict["DipDirection"].ToString("0.000", new CultureInfo("en-GB")) + ",";

            csvData += instance.SurfaceList[i].transform.position.z.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.SurfaceList[i].transform.position.x.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.SurfaceList[i].transform.position.y.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.SurfaceList[i].transform.rotation.z.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.SurfaceList[i].transform.rotation.x.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.SurfaceList[i].transform.rotation.y.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.SurfaceList[i].transform.rotation.w.ToString("0.0000", new CultureInfo("en-GB")) + ",";



            csvData += instance.creationDate.ToString("MM/dd/yyyy H:mm:ss.ffff") + ",";
            csvData += instance.CustomTxt;
            writer.WriteLine(csvData, "en-GB");
        }

       // writer.WriteLine(csvData, "en-GB");
        writer.Close();


        //kml

        var kml = new SharpKml.Dom.Kml();
        kml.AddNamespacePrefix(KmlNamespaces.GX22Prefix, KmlNamespaces.GX22Namespace);

        // Create style 1
        var style = new Style();
        style.Id = "s_ylw-pushpin";
        style.Icon = new IconStyle();
        style.Icon.Hotspot = new Hotspot();
        style.Icon.Hotspot.X = 20.0;
        style.Icon.Hotspot.Y = 2.0;
        style.Icon.Hotspot.XUnits = Unit.Pixel;
        style.Icon.Hotspot.YUnits = Unit.Pixel;
        style.Icon.Icon = new IconStyle.IconLink(new Uri("http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png"));
        style.Icon.Scale = 1.1;

        // Create style 2
        var style2 = new Style();
        style2.Id = "s_ylw-pushpin_hl";
        style2.Icon = new IconStyle();
        style2.Icon.Hotspot = new Hotspot();
        style2.Icon.Hotspot.X = 20.0;
        style2.Icon.Hotspot.Y = 2.0;
        style2.Icon.Hotspot.XUnits = Unit.Pixel;
        style2.Icon.Hotspot.YUnits = Unit.Pixel;
        style2.Icon.Icon = new IconStyle.IconLink(new Uri("http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png"));
        style2.Icon.Scale = 1.3;

        //stylemap
        var pair1 = new Pair();
        pair1.State = StyleState.Normal;
        pair1.StyleUrl = new Uri("#s_ylw-pushpin", UriKind.Relative);

        var pair2 = new Pair();
        pair2.State = StyleState.Highlight;
        pair2.StyleUrl = new Uri("#s_ylw-pushpin_hl", UriKind.Relative);

        var stylemap = new StyleMapCollection();
        stylemap.Id = "m_ylw - pushpin";
        stylemap.Add(pair1);
        stylemap.Add(pair2);

        // This will be the location of the Placemark.
        realPosition = VirtualMeter.CalculateGPSPosition(instance.SurfaceList[0].transform.position);
        var point = new Point();
        point.Coordinate = new Vector((double)realPosition.z, (double)realPosition.x, (double)realPosition.y);

        //point.Tilt = 40;

        // This is the Element to save to the Kml file.
        var placemark = new Placemark();
        placemark.Geometry = point;
        placemark.Name = instance.ToolTitle;
        placemark.Description = new Description() { Text = instance.CustomTxt };
        placemark.StyleUrl = new Uri("#m_ylw-pushpin", UriKind.Relative);

        // create the document
        var document = new SharpKml.Dom.Document();
        document.Name = instance.ToolTitle + " " + instance.ID;// creationDate.ToString("yyyy.MMddHmmssffff");
        document.AddStyle(stylemap);
        document.AddStyle(style);
        document.AddStyle(style2);
        document.AddFeature(placemark);

        // set root 
        kml.Feature = document;

        // save to file
        KmlFile kmlf = KmlFile.Create(kml, false);
        using (var stream = System.IO.File.OpenWrite(Path.Combine(directoryPath, document.Name + ".kml")))
        {
            kmlf.Save(stream);
        }


       // ToolController.globalToolControllerObject.StartCoroutine(Tool.ShowNotification("Single measure has been exported", 1));
    }

    public  void SaveMultiInstance()
    {

        if (instanceList.Count==0)
        {
            ToolController.globalToolControllerObject.StartCoroutine(Tool.ShowNotificationLabelForMesuring("No data!", 1.5f));
        }
        else
        {
            string FilePath = "Outputs/Surfaces";


            // CSV
            var directory = new DirectoryInfo(Application.dataPath);
            var directoryPath = Path.Combine(directory.Parent.FullName, FilePath);

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }

            var path = Path.Combine(directoryPath, "AllSurfaces " + DateTime.Now.ToString("yyyyMMdd_Hmmssffff") + ".csv");

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var sr = File.CreateText(path);
            string csvData = "";
            csvData += "#Index, Id, Lat, Lon, z, Strike, Dip, DipDirection, unity_z, unity_x,unity_y, unity_rotation_z, unity_rotation_x, unity_rotation_y, unity_rotation_w, Date, Comments";
            sr.WriteLine(csvData);
            Vector3Decimal realPosition = new Vector3Decimal();
            int j = 0;
            foreach (var p in instanceList)
            {
                for (int i = 0; i < p.GetComponent<ToolInstance>().SurfaceList.Count; ++i)
                {
                    csvData = "";
                    realPosition = VirtualMeter.CalculateGPSPosition(p.GetComponent<ToolInstance>().SurfaceList[i].transform.position);
                    csvData += j.ToString() + ",";
                    csvData += p.GetComponent<ToolInstance>().ID;//.creationDate.ToString("yyyy.MMddHmmssffff") + ",";
                    csvData += ((double)realPosition.z).ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                    csvData += ((double)realPosition.x).ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                    csvData += ((double)realPosition.y).ToString("0.000", new CultureInfo("en-GB")) + ",";
                    csvData += p.GetComponent<ToolInstance>().ValueDict["Strike"].ToString("0.000", new CultureInfo("en-GB")) + ",";
                    csvData += p.GetComponent<ToolInstance>().ValueDict["Dip"].ToString("0.000", new CultureInfo("en-GB")) + ",";
                    csvData += p.GetComponent<ToolInstance>().ValueDict["DipDirection"].ToString("0.000", new CultureInfo("en-GB")) + ",";

                    csvData += p.GetComponent<ToolInstance>().SurfaceList[i].transform.position.z.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += p.GetComponent<ToolInstance>().SurfaceList[i].transform.position.x.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += p.GetComponent<ToolInstance>().SurfaceList[i].transform.position.y.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += p.GetComponent<ToolInstance>().SurfaceList[i].transform.rotation.z.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += p.GetComponent<ToolInstance>().SurfaceList[i].transform.rotation.x.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += p.GetComponent<ToolInstance>().SurfaceList[i].transform.rotation.y.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += p.GetComponent<ToolInstance>().SurfaceList[i].transform.rotation.w.ToString("0.0000", new CultureInfo("en-GB")) + ",";


                    csvData += p.GetComponent<ToolInstance>().creationDate.ToString("MM/dd/yyyy H:mm:ss.ffff") + ",";
                    csvData += p.GetComponent<ToolInstance>().CustomTxt;
                    sr.WriteLine(csvData);
                }
                j++;
            }

            //sr.WriteLine(csvData);
            sr.Close();


            // KML

            var kml = new SharpKml.Dom.Kml();
            kml.AddNamespacePrefix(KmlNamespaces.GX22Prefix, KmlNamespaces.GX22Namespace);

            // Create style 1
            var style = new Style();
            style.Id = "s_ylw-pushpin";
            style.Icon = new IconStyle();
            style.Icon.Hotspot = new Hotspot();
            style.Icon.Hotspot.X = 20.0;
            style.Icon.Hotspot.Y = 2.0;
            style.Icon.Hotspot.XUnits = Unit.Pixel;
            style.Icon.Hotspot.YUnits = Unit.Pixel;
            style.Icon.Icon = new IconStyle.IconLink(new Uri("http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png"));
            style.Icon.Scale = 1.1;

            // Create style 2
            var style2 = new Style();
            style2.Id = "s_ylw-pushpin_hl";
            style2.Icon = new IconStyle();
            style2.Icon.Hotspot = new Hotspot();
            style2.Icon.Hotspot.X = 20.0;
            style2.Icon.Hotspot.Y = 2.0;
            style2.Icon.Hotspot.XUnits = Unit.Pixel;
            style2.Icon.Hotspot.YUnits = Unit.Pixel;
            style2.Icon.Icon = new IconStyle.IconLink(new Uri("http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png"));
            style2.Icon.Scale = 1.3;

            //stylemap
            var pair1 = new Pair();
            pair1.State = StyleState.Normal;
            pair1.StyleUrl = new Uri("#s_ylw-pushpin", UriKind.Relative);

            var pair2 = new Pair();
            pair2.State = StyleState.Highlight;
            pair2.StyleUrl = new Uri("#s_ylw-pushpin_hl", UriKind.Relative);

            var stylemap = new StyleMapCollection();
            stylemap.Id = "m_ylw - pushpin";
            stylemap.Add(pair1);
            stylemap.Add(pair2);

            // create the document
            var document = new SharpKml.Dom.Document();
            document.Name = "AllSurfaces " + DateTime.Now.ToString("yyyyMMdd_Hmmssffff");
            document.AddStyle(stylemap);
            document.AddStyle(style);
            document.AddStyle(style2);

            // This will be the location of the Placemark.
            foreach (var p in instanceList)
            {
                realPosition = VirtualMeter.CalculateGPSPosition(p.GetComponent<ToolInstance>().SurfaceList[0].transform.position);
                var point = new Point();
                point.Coordinate = new Vector((double)realPosition.z, (double)realPosition.x, (double)realPosition.y);

                // This is the Element to save to the Kml file.
                var placemark = new Placemark();
                placemark.Geometry = point;
                placemark.Name = p.GetComponent<ToolInstance>().ToolTitle + " " + p.GetComponent<ToolInstance>().creationDate.ToString("yyyy.MMddHmmssffff");
                placemark.Description = new Description() { Text = p.GetComponent<ToolInstance>().CustomTxt };
                placemark.StyleUrl = new Uri("#m_ylw-pushpin", UriKind.Relative);

                document.AddFeature(placemark);
            }

            // set root 
            kml.Feature = document;

            // save to file
            KmlFile kmlf = KmlFile.Create(kml, false);
            using (var stream = System.IO.File.OpenWrite(Path.Combine(directoryPath, document.Name + ".kml")))
            {
                kmlf.Save(stream);
            }

            ToolController.globalToolControllerObject.StartCoroutine(Tool.ShowNotificationLabelForMesuring("Done!", 1.5f));
            //ToolController.globalToolControllerObject.StartCoroutine(Tool.ShowNotification("Multiple surfaces have been exported", 1));
        }
    }

    public void DeleteAllInstances()
    {

        foreach (var p in instanceList)
        {


        
            for (int i = 0; i < p.GetComponent<ToolInstance>().SurfaceList.Count; ++i)
            {
                
                p.GetComponent<ToolInstance>().DestroySingleInstance();
            }

            localID--;

        }
        instanceList.Clear();
        surfaceMap.Clear();

    }

    public void ShowHide()
    {

        bool status = true;
        if (instanceList.Count > 0)
            status = instanceList[0].GetComponent<ToolInstance>().SurfaceList[0].activeSelf;

        foreach (var p in instanceList)
        {
            for (int i = 0; i < p.GetComponent<ToolInstance>().SurfaceList.Count; ++i)
            {
                p.GetComponent<ToolInstance>().SurfaceList[i].SetActive(!status);
            }
        }

        string statusString = !status ? "Hide" : "Show";
        GameObject.Find("Canvas").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = statusString;

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = statusString;

    }


    public void OnPointerExit()
    {

        GameObject.Find("Canvas").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = "";

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = "";

        //throw new System.NotImplementedException();
    }

    public void OnPointerEnter()
    {
        string statusString = "Hide / Show";
        if (instanceList.Count > 0)
            statusString = instanceList[0].GetComponent<ToolInstance>().SurfaceList[0].activeSelf ? "Hide" : "Show";

        GameObject.Find("Canvas").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = statusString;

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = statusString;

    }
}
