using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
//using UnityEditor.Events;
using SharpKml;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System.Globalization;
using System.Text.RegularExpressions;

/// <summary>
/// This Tool creates a sequence of connecting placemarks
/// 
/// each placemark has a line renderer, that connects to the placemark connected after it in the chain. 
/// the placemarks can be though of as a doubly linked list, each stores reference to the one before and after it in the list. 
/// 
/// </summary>
/// <returns></returns>
/// 
public class PolylineTool : Tool
{

    public static int localID = 0;

    [HideInInspector]
    public static List<GameObject> instanceList = new List<GameObject>();
    [HideInInspector]
    public static List<string> LineMap = new List<string>();

    private string _note = "";
    string oldNote = "";

    public override IEnumerator ToolCoroutine( )
    {

       

        // a List to store all created placemarks in the line
        //List<Vector3> placemarks = new List<Vector3>();
        List<GameObject> placemarks = new List<GameObject>();

        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();
        ToolController.ToolIsCurrentlyRunning = true;
        ToolController.ToolControllerInterfaceIsCurrentlyRunning = false;



        toolControllerComponent.MeasurementControlUI.gameObject.SetActive(false);
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("ToolMenu").gameObject.transform.Find("Group2").gameObject.transform.Find("Field_notebook").gameObject.GetComponent<FieldNotes>().outputString = "";

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.SetActive(false);
        }
/*
        // hold until trigger is released
        // this avoids instant placement 
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
        {
            while (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.8f)
            {
                yield return wfeof;
            }
        }
*/

        // temporary objects
        GameObject TempPlacemark, LastPlaceMark = null, TempPlacemarkInfo = null;
        LineRenderer lr = null;
        Vector3Decimal realPosition;

        // loop until a user chooses to cancel the tool 
        bool shouldStopPlacing = false;
        while (!shouldStopPlacing)
        {
            //Instantiate placemark
            TempPlacemark = Instantiate( toolControllerComponent.PlacemarkObject2 );
            TempPlacemark.transform.localScale *= toolControllerComponent.MarkerScale;
            TempPlacemark.name = "distance placemark";
            bool placemarkConfirmed = false, legalPlacemarkPlace = false;
            bool wasHolding = true;

          
            if (LastPlaceMark != null)
            {
                lr = TempPlacemark.AddComponent<LineRenderer>() as LineRenderer;
                lr.startWidth = lr.endWidth = (float)(toolControllerComponent.MarkerScale * 0.2f);

                lr.material = toolControllerComponent.LineMaterial;

                //lr.SetPosition(1, TempPlacemark.transform.position + Vector3.up * (float)(toolControllerComponent.MarkerScale * 0.5f));
                lr.SetPosition(1, TempPlacemark.transform.position );
                lr.SetPosition(0, LastPlaceMark.transform.position );
            }

            //Handling Placemark
            RaycastHit hit;
            while (placemarkConfirmed == false && shouldStopPlacing == false)
            {
                if (StateSingleton.stateView != StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView != StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
                {
                    if (wasHolding == true && OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) < 0.2f)
                        wasHolding = false;
                }
                //Trailing Waypoint
                if (Physics.Raycast(master.transform.position, directionMaster.transform.forward, out hit, 100000))
                {
                    //pos 
                    //TempPlacemark.transform.position = hit.point + (toolControllerComponent.MarkerScale * 2) * Vector3.up;
                    TempPlacemark.transform.position = hit.point;
                    //rotation
                    TempPlacemark.transform.eulerAngles = new Vector3(0f, directionMaster.transform.eulerAngles.y, directionMaster.transform.eulerAngles.z);                    
                    legalPlacemarkPlace = true;

                    //Showing info on toolInfo
                    realPosition = VirtualMeter.CalculateRealPositionOfPoint(TempPlacemark.transform.position - 40 * Vector3.up);                   
                    toolControllerComponent.updateGUIMenus("Current Placemark",
                        "Northing (m): " + realPosition.z.ToString("0.000") +
                        "\nEasting (m): " + realPosition.x.ToString("0.000") +
                        "\nAltitude (m): " + realPosition.y.ToString("0.000")
                        );
                }
                else
                {
                    //If not well placed then invalidate
                    TempPlacemark.transform.position = master.transform.position + directionMaster.transform.forward * 100000;
                    toolControllerComponent.updateGUIMenus("Current Placemark", "Northing (m): undefined \nEasting (m): undefined \nAltitude (m): undefined" );
                    legalPlacemarkPlace = false;  
                }

                if (lr)
                {
                    lr.SetPosition(1, TempPlacemark.transform.position);
                }

                //confirm waypoint
                if (legalPlacemarkPlace && ( Input.GetMouseButton(0) ||
                (
                    (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS) &&
                    OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.5f && wasHolding == false )
                    )
                )
                {
                    placemarkConfirmed = true;
                    placemarks.Add(TempPlacemark);
                    LastPlaceMark = TempPlacemark;
                }
                
                // whether to cancel out of tool
                // this should also remove potential placemarks
                if (checkIfToolShouldQuit())// || OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0.90f)
                {
                    shouldStopPlacing = true;
                    Destroy(TempPlacemark);

                    toolControllerComponent.OculusMasterObject.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.2783019f, 0.3061422f, 1, 1);

                    if (placemarks.Count < 2)
                    {
                        ToolController.ToolIsCurrentlyRunning = false;
                        ToolController.ToolControllerInterfaceIsCurrentlyRunning = true;

                        toolControllerComponent.MeasurementControlUI.gameObject.SetActive(true);
                        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
                            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.SetActive(true);
                    }
                }

                yield return wfeof;
            }            
        }

        if (placemarks.Count > 1)
        {
            localID++;
            //do calculations
            Vector3 uPos1;
            Vector3 uPos2;

            decimal totalDistance = 0m;
            decimal totalDistance_2d = 0m;

            Vector3Decimal meanPosition = new Vector3Decimal(0m, 0m, 0m);

            for (int i = 0; i < placemarks.Count; i++)
            {
                uPos1 = placemarks[i].transform.position;
                if (i < placemarks.Count - 1)
                {
                    uPos2 = placemarks[i + 1].transform.position;
                    totalDistance += VirtualMeter.CalculateRealDistance(uPos1, uPos2);
                    totalDistance_2d += VirtualMeter.CalculateRealDistance2D(uPos1, uPos2);
                }
                meanPosition += VirtualMeter.CalculateRealPositionOfPoint(uPos1);
            }
            meanPosition /= placemarks.Count;
            
            // create value ditionary
            Dictionary<string, decimal> dict = new Dictionary<string, decimal>()
                                                {
                {"Total_Distance(m)",      totalDistance},
                {"Total_Distance_2D(m)",      totalDistance_2d}
                //,{ "Mean_Northing (m)" ,    meanPosition.z }
                //,{ "Mean_Easting (m)" ,     meanPosition.x }
                //,{ "Mean_Altitude (m)" ,    meanPosition.y }
                                                };


            oldNote = "\n3D Length (m): " + totalDistance.ToString("0.000") + 
                "\n2D Length (m): "+ totalDistance_2d.ToString("0.000") +
                "\nNote: ";

            DateTime date = DateTime.Now;
            string globalID = date.ToString("yyyy.MMddHmmssffff") + ",";


            // create tool instance    
            toolControllerComponent.CreateToolInstance("Line",
                oldNote, "",
                Tool.toolType.LINE,
                dict, placemarks, date,
                false, false, true, localID, globalID);
        }
        else
        {
            foreach(var p in placemarks)
            {
                Destroy(p);
            }
            ToolController.ToolIsCurrentlyRunning = false;   
        }     
    }
    
    public void startToolInterface()
    {

      
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_line").gameObject.SetActive(true);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_surface").gameObject.SetActive(false);

        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_line").gameObject.SetActive(true);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_surface").gameObject.SetActive(false);


        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_line").gameObject.SetActive(true);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_surface").gameObject.SetActive(false);

        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_line").gameObject.SetActive(true);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_surface").gameObject.SetActive(false);

        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_surface").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_line").gameObject.SetActive(true);

        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_line").gameObject.SetActive(true);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_surface").gameObject.SetActive(false);



        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("UpperPanel").gameObject.transform.Find("Text").gameObject.GetComponent<Text>().text = "Line tool";

        GameObject.Find("Canvas").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();

        GameObject.Find("Canvas").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.AddListener(GetInsertedNotes);


        toolControllerComponent.MeasurementControlUI.SetActive(true);
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("UpperPanel").gameObject.transform.Find("Text").gameObject.GetComponent<Text>().text = "Line tool";
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.SetActive(true);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_line").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_ruler").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_surface").gameObject.SetActive(false);



            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_line").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_ruler").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_surface").gameObject.SetActive(false);


            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_line").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_ruler").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_surface").gameObject.SetActive(false);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_line").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_ruler").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_surface").gameObject.SetActive(false);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_line").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_ruler").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_surface").gameObject.SetActive(false);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_line").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_ruler").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_surface").gameObject.SetActive(false);



            //   GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash").gameObject.GetComponent<Button>().onClick.AddListener(DeleteAllInstances);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.AddListener(GetInsertedNotes);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").transform.localPosition = new Vector3(0.0f, -400.0f, 0.0f);

        }
        PauseAndGUIBehaviour.isToolMenu = false;
        ToolController.ToolControllerInterfaceIsCurrentlyRunning = true;


        //  StartCoroutine(this.OnUse());
    }


    public void GetInsertedNotes()
    {
         _note = GameObject.Find("Canvas_Oculus").gameObject.transform.Find("ToolMenu").gameObject.transform.Find("Group2").gameObject.transform.Find("Field_notebook").gameObject.GetComponent<FieldNotes>().outputString;


      
        if (instanceList.Count != 0)
        {

        
            ToolInstance currentInstance = instanceList[instanceList.Count - 1].GetComponent<ToolInstance>();
            currentInstance.CustomTxt = _note;

           

          

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



        var path = Path.Combine(directoryPath, string.Format("lines.csv"));


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
            string prevId = "";
            List<GameObject> placemarks = null;
            string date = "", comment = "";

            foreach (String line in lines)
            {
                String[] data = line.Split(","[0]);


                if (data.Length == 16 && data[0][0].ToString() != "#") // discard non-coordinate lines
                {
                    if (prevId != data[1])
                    {

                        if (placemarks != null)
                            createLineInImport(placemarks, prevId, date, comment);

                        prevId = data[1];
                        placemarks = new List<GameObject>();


                    }



                    decimal z = decimal.Parse(data[7], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB"));
                    decimal x = decimal.Parse(data[8], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB"));
                    decimal y = decimal.Parse(data[9], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB"));

                    decimal rot_z = decimal.Parse(data[10], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB"));
                    decimal rot_x = decimal.Parse(data[11], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB"));
                    decimal rot_y = decimal.Parse(data[12], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB"));
                    decimal rot_w = decimal.Parse(data[13], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB"));

                    date = data[14];
                    comment = Regex.Replace(data[15], @"\t|\n|\r", "");


                    TempPlacemark = Instantiate(toolControllerComponent.PlacemarkObject2);
                    TempPlacemark.transform.localScale *= toolControllerComponent.MarkerScale;
                    TempPlacemark.name = "distance placemark";

                    //TempPlacemark.transform.position = new Vector3((float)unityPosition.z, (float)unityPosition.x, (float)unityPosition.y); // this will need to account for offset
                    TempPlacemark.transform.position = new Vector3((float)x, (float)y, (float)z); // this will need to account for offset
                    TempPlacemark.transform.rotation = new Quaternion((float)rot_x, (float)rot_y, (float)rot_z, (float)rot_w); // this will need to account for offset
                    placemarks.Add(TempPlacemark);

                }
            }

            //Add the latest Polygon
            createLineInImport(placemarks, prevId, date, comment);



        }
        return null;
    }


    private void createLineInImport(List<GameObject> placemarks, string name, string date, string comment)
    {



        //test if already imported
        if (!LineMap.Contains(name))
        {



            //do calculations
            GameObject TempPlacemark;

            LineRenderer lr = null;

            decimal totalDistance = 0m;
            decimal totalDistance_2d = 0m;


            /*

           */

            Vector3Decimal meanPosition = new Vector3Decimal(0m, 0m, 0m);

            for (int i = 0; i < placemarks.Count - 1; i++)
            {
                lr = placemarks[i].AddComponent<LineRenderer>() as LineRenderer;
                lr.startWidth = lr.endWidth = (float)(toolControllerComponent.MarkerScale * 0.2f);

                lr.material = toolControllerComponent.LineMaterial;

                //lr.SetPosition(1, TempPlacemark.transform.position + Vector3.up * (float)(toolControllerComponent.MarkerScale * 0.5f));
                lr.SetPosition(1, placemarks[i + 1].transform.position);
                lr.SetPosition(0, placemarks[i].transform.position);
            }

            // create value ditionary
            Dictionary<string, decimal> dict = new Dictionary<string, decimal>()
                                                {
                {"Total_Distance(m)",      totalDistance},
                {"Total_Distance_2D(m)",      totalDistance_2d}
                //,{ "Mean_Northing (m)" ,    meanPosition.z }
                //,{ "Mean_Easting (m)" ,     meanPosition.x }
                //,{ "Mean_Altitude (m)" ,    meanPosition.y }
                                                };


            oldNote = "\n3D Length (m): " + totalDistance.ToString("0.000") +
                "\n2D Length(m):" + totalDistance_2d.ToString("0.000") +
                "\nNote: ";


            DateTime myDate = DateTime.ParseExact(date, "MM/dd/yyyy H:mm:ss.ffff",
                                  System.Globalization.CultureInfo.InvariantCulture);


            toolControllerComponent.CreateToolInstance("Polygon", oldNote
                , "",
                Tool.toolType.LINE,
                dict, placemarks, myDate, false, false, true, localID, name, false);


            localID++;
        }
        else
        {
            Debug.Log("elemento già presente");

        }




    }

    public static void SaveSingleInstance(ToolInstance instance)
    {
        string FilePath = "Outputs/Lines";


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
        // csvData += "#Index, Id, Lat, Lon, z, Date, Comments";
        csvData += "#Index, Id, Lat, Lon, z, 2D Length, 3D Length, unity_z, unity_x,unity_y, unity_rotation_z, unity_rotation_x, unity_rotation_y,unity_rotation_w, Date, Comments";

        writer.WriteLine(csvData, "en-GB");
        Vector3Decimal realPosition = new Vector3Decimal();
        for (int i = 0; i < instance.PolylineList.Count; ++i)
        {
            realPosition = VirtualMeter.CalculateGPSPosition(instance.PolylineList[i].transform.position);

         

            csvData = i.ToString() + ",";
            csvData += instance.ID;//creationDate.ToString("yyyy.MMddHmmssffff") + ",";
            csvData += realPosition.z.ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
            csvData += realPosition.x.ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
            csvData += realPosition.y.ToString("0.000", new CultureInfo("en-GB")) + ",";

            csvData += instance.ValueDict["Total_Distance(m)"].ToString("0.000", new CultureInfo("en-GB")) + ",";
            csvData += instance.ValueDict["Total_Distance_2D(m)"].ToString("0.000", new CultureInfo("en-GB")) + ",";

            csvData += instance.PolylineList[i].transform.position.z.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.PolylineList[i].transform.position.x.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.PolylineList[i].transform.position.y.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.PolylineList[i].transform.rotation.z.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.PolylineList[i].transform.rotation.x.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.PolylineList[i].transform.rotation.y.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.PolylineList[i].transform.rotation.w.ToString("0.0000", new CultureInfo("en-GB")) + ",";



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
        
        // construct the linestring
        LineString line = new LineString();
        line.Coordinates = new CoordinateCollection();
        line.Tessellate = true;

        foreach (var pl in instance.GetComponent<ToolInstance>().PolylineList)
        {
            realPosition = VirtualMeter.CalculateGPSPosition(pl.transform.position);
            Vector linePoint = new Vector((double)realPosition.z, (double)realPosition.x, (double)realPosition.y);
            line.Coordinates.Add(linePoint);
        }

        var placemark = new Placemark();
        placemark.Geometry = line;
        placemark.Name = instance.ToolTitle;
        placemark.Description = new Description() { Text = instance.CustomTxt };
        placemark.StyleUrl = new Uri("#m_ylw-pushpin", UriKind.Relative);

        // create the document
        var document = new SharpKml.Dom.Document();
        document.Name = instance.ToolTitle + " " + instance.ID;//creationDate.ToString("yyyy.MMddHmmssffff");
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


        //ToolController.globalToolControllerObject.StartCoroutine(Tool.ShowNotification("Single measure has been exported", 1));
    }

    public void SaveMultiInstance()
    {

        if (instanceList.Count == 0)
        {
            ToolController.globalToolControllerObject.StartCoroutine(Tool.ShowNotificationLabelForMesuring("No data!", 1.5f));
        }
        else
        {
            string FilePath = "Outputs/Lines";


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

            var path = Path.Combine(directoryPath, "AllLines " + DateTime.Now.ToString("yyyyMMdd_Hmmssffff") + ".csv");

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var sr = File.CreateText(path);
            string csvData = "";
            //csvData += "#Index, Id, Lat, Lon, z, Date, Comments";
            csvData += "#Index, Id, Lat, Lon, z, 2D Length, 3D Length, unity_z, unity_x,unity_y, unity_rotation_z, unity_rotation_x, unity_rotation_y,unity_rotation_w, Date, Comments";

            sr.WriteLine(csvData, "en-GB");
            int j = 0;
            Vector3Decimal realPosition = new Vector3Decimal();
            foreach (var inst in instanceList)
            {
                for (int i = 0; i < inst.GetComponent<ToolInstance>().PolylineList.Count; ++i)
                {
                    realPosition = VirtualMeter.CalculateGPSPosition(inst.GetComponent<ToolInstance>().PolylineList[i].transform.position);
                    csvData = j.ToString() + ",";
                    csvData += inst.GetComponent<ToolInstance>().ID;// creationDate.ToString("yyyy.MMddHmmssffff") + ",";
                    csvData += ((double)realPosition.z).ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                    csvData += ((double)realPosition.x).ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                    csvData += ((double)realPosition.y).ToString("0.000", new CultureInfo("en-GB")) + ",";


                    csvData += inst.GetComponent<ToolInstance>().ValueDict["Total_Distance(m)"].ToString("0.000", new CultureInfo("en-GB")) + ",";
                    csvData += inst.GetComponent<ToolInstance>().ValueDict["Total_Distance_2D(m)"].ToString("0.000", new CultureInfo("en-GB")) + ",";

                    csvData += inst.GetComponent<ToolInstance>().PolylineList[i].transform.position.z.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += inst.GetComponent<ToolInstance>().PolylineList[i].transform.position.x.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += inst.GetComponent<ToolInstance>().PolylineList[i].transform.position.y.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += inst.GetComponent<ToolInstance>().PolylineList[i].transform.rotation.z.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += inst.GetComponent<ToolInstance>().PolylineList[i].transform.rotation.x.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += inst.GetComponent<ToolInstance>().PolylineList[i].transform.rotation.y.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += inst.GetComponent<ToolInstance>().PolylineList[i].transform.rotation.w.ToString("0.0000", new CultureInfo("en-GB")) + ",";


                    csvData += inst.GetComponent<ToolInstance>().creationDate.ToString("MM/dd/yyyy H:mm:ss.ffff") + ",";
                    csvData += inst.GetComponent<ToolInstance>().CustomTxt;
                    sr.WriteLine(csvData, "en-GB");
                    j++;

                }
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
            document.Name = "AllLines " + DateTime.Now.ToString("yyyyMMdd_Hmmssffff");
            document.AddStyle(stylemap);
            document.AddStyle(style);
            document.AddStyle(style2);

            // This will be the location of the Placemark.
            foreach (var inst in instanceList)
            {
                // construct the linestring
                LineString line = new LineString();
                line.Coordinates = new CoordinateCollection();
                line.Tessellate = true;

                foreach (var pl in inst.GetComponent<ToolInstance>().PolylineList)
                {
                    realPosition = VirtualMeter.CalculateGPSPosition(pl.transform.position);
                    Vector linePoint = new Vector((double)realPosition.z, (double)realPosition.x, (double)realPosition.y);
                    line.Coordinates.Add(linePoint);
                }

                var placemark = new Placemark();
                placemark.Geometry = line;
                placemark.Name = inst.GetComponent<ToolInstance>().ToolTitle + " " + inst.GetComponent<ToolInstance>().creationDate.ToString("yyyy.MMddHmmssffff");
                placemark.Description = new Description() { Text = inst.GetComponent<ToolInstance>().CustomTxt };
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
        }
    }



    public  void DeleteAllInstances()
    {
       
        foreach (var p in instanceList)
        {


        
            for (int i = 0; i < p.GetComponent<ToolInstance>().PolylineList.Count; ++i)
            {
                Debug.Log("sono: "+p.GetComponent<ToolInstance>().instanceToolType);
                p.GetComponent<ToolInstance>().DestroySingleInstance();
            }

            localID--;

        }
        instanceList.Clear();

    }

    public void ShowHide()
    {

        bool status = true;
        if (instanceList.Count > 0)
            status = instanceList[0].GetComponent<ToolInstance>().PolylineList[0].activeSelf;

        foreach (var p in instanceList)
        {
            for (int i = 0; i < p.GetComponent<ToolInstance>().PolylineList.Count; ++i)
            {


                p.GetComponent<ToolInstance>().PolylineList[i].SetActive(!status);
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
            statusString = instanceList[0].GetComponent<ToolInstance>().PolylineList[0].activeSelf ? "Hide" : "Show";

        GameObject.Find("Canvas").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = statusString;

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = statusString;

    }




}