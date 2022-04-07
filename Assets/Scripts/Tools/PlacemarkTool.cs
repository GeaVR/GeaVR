/******************************************************************************
 *
 *                      GeaVR
 *                https://www.geavr.eu/
 *             https://github.com/GeaVR/GeaVR
 * 
 * GeaVR is an open source software that allows the user to experience a wide 
 * range of geological and geomorphological sites in immersive virtual reality,
 * including data collection.
 *
 * Main Developers:      
 * 
 *     Fabio Luca Bonali (fabio.bonali@unimib.it)
 *     Martin Kearl (martintkearl@gmail.com)
 *     Fabio Roberto Vitello (fabio.vitello@inaf.it)
 * 
 * Developed thanks to the contribution of following projects:
 *
 *     ACPR15T4_ 00098 “Agreement between the University of Milan Bicocca and the 
 *     Cometa Consortium for the experimentation of cutting-edge interactive 
 *     technologies for the improvement of science teaching and dissemination” of 
 *     Italian Ministry of Education, University and Research (ARGO3D)
 *     PI: Alessandro Tibaldi (alessandro.tibaldi@unimib.it)
 *     
 *     Erasmus+ Key Action 2 2017-1-UK01-KA203- 036719 “3DTeLC – Bringing the  
 *     3D-world into the classroom: a new approach to Teaching, Learning and 
 *     Communicating the science of geohazards in terrestrial and marine 
 *     environments”
 *     PI: Malcolm Whitworth (malcolm.Whitworth@port.ac.uk)
 * 
 ******************************************************************************
 * Copyright (c) 2016-2022
 * GPL-3.0 License
 *****************************************************************************/

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

public class PlacemarkTool : Tool
{
    public static int localID = 0;

    [HideInInspector]
    public static List<GameObject> instanceList = new List<GameObject>();
    [HideInInspector]
    public static List<string> PlacemarkMap = new List<string>();

    private string _note="";
    string oldNote = "";

    public override IEnumerator ToolCoroutine()
    {

        
        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();
        ToolController.ToolIsCurrentlyRunning = true;
        ToolController.ToolControllerInterfaceIsCurrentlyRunning = false;

        toolControllerComponent.MeasurementControlUI.gameObject.SetActive(false);
            
        // hold until trigger is released
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("ToolMenu").gameObject.transform.Find("Group2").gameObject.transform.Find("Field_notebook").gameObject.GetComponent<FieldNotes>().outputString = "";

            while (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.2f)
            {
                yield return wfeof;
            }
        }

        // temporary objects
        GameObject TempPlacemark, LastPlaceMark = null, TempPlacemarkInfo = null;
        List<GameObject> placemarks = new List<GameObject>();

        //Instantiate placemark
        TempPlacemark = Instantiate(toolControllerComponent.PlacemarkObject);
        TempPlacemark.transform.localScale *= toolControllerComponent.MarkerScale;
        TempPlacemark.name = "placemark";
       
        
        //boolean flags
        bool placemarkConfirmed = false;
        bool legalPlacemarkPlace = false;
        Vector3Decimal realPosition;

        //Handling Placemark
        RaycastHit hit;
        while (placemarkConfirmed == false)
        {
            //Trailing Waypoint
            if (Physics.Raycast(master.transform.position, directionMaster.transform.forward, out hit, 100000))
            {
                TempPlacemark.transform.position = hit.point + (toolControllerComponent.MarkerScale * 2) * Vector3.up;
                TempPlacemark.transform.eulerAngles = new Vector3(0f, directionMaster.transform.eulerAngles.y, directionMaster.transform.eulerAngles.z);
                legalPlacemarkPlace = true;
               
                //Showing info on toolInfo
                realPosition = VirtualMeter.CalculateGPSPosition(TempPlacemark.transform.position);

                toolControllerComponent.updateGUIMenus("Placemark",
                    "Lat(°): " + realPosition.z.ToString("0.0000000000000") +
                    "\nLon(°): " + realPosition.x.ToString("0.0000000000000") +
                    "\nAltitude (m): " + realPosition.y.ToString("0.000")
                );
            }
            else
            {
                //If not well placed then invalidate
                TempPlacemark.transform.position = master.transform.position + directionMaster.transform.forward * 100000;
                toolControllerComponent.updateGUIMenus("Placemark", "Lat(°): undefined \nLon(°): undefined \nAltitude (m): undefined");
                legalPlacemarkPlace = false;
            }

            if (
            (
            Input.GetMouseButton(0) ||
                (
                    (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS) &&
                    OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.5f)
            )
            && legalPlacemarkPlace)
            {
                placemarkConfirmed = true;
                toolControllerComponent.OculusMasterObject.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.2783019f, 0.3061422f, 1, 1);

            }

            if (checkIfToolShouldQuit())
            {
                placemarkConfirmed = true;
                Destroy(TempPlacemark);
                ToolController.ToolIsCurrentlyRunning = false;
                ToolController.ToolControllerInterfaceIsCurrentlyRunning = true;

                toolControllerComponent.OculusMasterObject.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.2783019f, 0.3061422f, 1, 1);


                toolControllerComponent.MeasurementControlUI.gameObject.SetActive(true);
                if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
                    GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.SetActive(true);


                yield break; // end coroutine early 
            }
            yield return wfeof;
        }

        placemarks.Add(TempPlacemark);
        localID++;
       
        //do calculations
        realPosition = VirtualMeter.CalculateGPSPosition(TempPlacemark.transform.position);

        // create value ditionary
        Dictionary<string, decimal> dict = new Dictionary<string, decimal>()
                                            {
            { "Lat(°)" ,    realPosition.z },
            { "Lon(°)" ,     realPosition.x },
            { "Altitude (m)" ,    realPosition.y },
                                            };


        oldNote = "Lat(°): " + realPosition.z.ToString("0.0000000000000") +
            "\nLon(°): " + realPosition.x.ToString("0.0000000000000") +
            "\nAltitude (m): " + realPosition.y.ToString("0.000")+
            "\nNote: ";

        // create tool instance
        DateTime date = DateTime.Now;
        string globalID = date.ToString("yyyy.MMddHmmssffff") + ",";
        toolControllerComponent.CreateToolInstance("Placemark",
            oldNote, "",
			Tool.toolType.PLACEMARK,
            dict, placemarks, date,
            false,false,true,localID,globalID);

        //ToolController.ToolIsCurrentlyRunning = false;
    }

    public void startToolInterface()
    {
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("UpperPanel").gameObject.transform.Find("Text").gameObject.GetComponent<Text>().text = "Placemark tool";

        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_placemark").gameObject.SetActive(true);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_line").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_surface").gameObject.SetActive(false);

        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_placemark").gameObject.SetActive(true);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_line").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_surface").gameObject.SetActive(false);

        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_placemark").gameObject.SetActive(true);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_line").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_surface").gameObject.SetActive(false);

        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_placemark").gameObject.SetActive(true);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_line").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_surface").gameObject.SetActive(false);

        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_placemark").gameObject.SetActive(true);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_line").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_surface").gameObject.SetActive(false);


        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_placemark").gameObject.SetActive(true);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_line").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_ruler").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_surface").gameObject.SetActive(false);




        GameObject.Find("Canvas").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.Find("Canvas").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.AddListener(GetInsertedNotes);
      
        //GameObject.Find("Canvas").gameObject.transform.Find("ToolMenu").gameObject.transform.Find("Group2").gameObject.transform.Find("Field_notebook").gameObject.GetComponent<FieldNotes>().setOutputString(CustomTxt);




        toolControllerComponent.MeasurementControlUI.SetActive(true);
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("UpperPanel").gameObject.transform.Find("Text").gameObject.GetComponent<Text>().text = "Placemark tool";
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.SetActive(true);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_placemark").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_ruler").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_surface").gameObject.SetActive(false);

          

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_placemark").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_ruler").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_surface").gameObject.SetActive(false);


            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_placemark").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_ruler").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_surface").gameObject.SetActive(false);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_placemark").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_ruler").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_surface").gameObject.SetActive(false);


            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_placemark").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_ruler").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_surface").gameObject.SetActive(false);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_placemark").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_ruler").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_surface").gameObject.SetActive(false);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").transform.localPosition = new Vector3(0.0f, -400.0f, 0.0f);

            // GameObject.Find("Canvas_Oculus").gameObject.transform.Find("ToolMenu").gameObject.transform.Find("Group2").gameObject.transform.Find("Field_notebook").gameObject.GetComponent<FieldNotes>().setOutputString(CustomTxt);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.AddListener(GetInsertedNotes);

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

          
            ToolInstance currentInstance=instanceList[instanceList.Count - 1].GetComponent<ToolInstance>();
            currentInstance.CustomTxt = _note;

           

            if (GameObject.Find(currentInstance.GUIMenu.name) != null)
                GameObject.Find(currentInstance.GUIMenu.name).gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("InputField").GetComponent<InputField>().text = oldNote+""+ currentInstance.CustomTxt;
  
            if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
                GameObject.Find(currentInstance.OculusMenu.name).gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("InputField").GetComponent<InputField>().text = oldNote + "" + currentInstance.CustomTxt;



        }
    }

    public void CancelButton()
    {
        // toolControllerComponent.ToolMenuInstance.GetComponent<CanvasGroup>().alpha = 1;

        // StopGPSTracking(false);
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

        

        var path = Path.Combine(directoryPath, string.Format("placemarks.csv"));

       
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
                Debug.Log("line: "+line);
                String[] data = line.Split(","[0]);

                if ( data.Length == 14  && data[0][0].ToString() != "#") // discard non-coordinate lines
                {

                    if (!PlacemarkMap.Contains(data[1]))
                    {
                        List<GameObject> placemarks = new List<GameObject>();

                        decimal z = decimal.Parse(data[5], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB"));
                        decimal x = decimal.Parse(data[6], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB"));
                        decimal y = decimal.Parse(data[7], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB"));

                        decimal rot_z = decimal.Parse(data[8], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB"));
                        decimal rot_x = decimal.Parse(data[9], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB"));
                        decimal rot_y = decimal.Parse(data[10], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB"));
                        decimal rot_w = decimal.Parse(data[11], NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.Any, new CultureInfo("en-GB"));

                        string date = data[12];
                        string comment = Regex.Replace(data[13], @"\t|\n|\r", "");


                        TempPlacemark = Instantiate(toolControllerComponent.PlacemarkObject);
                        TempPlacemark.transform.localScale *= toolControllerComponent.MarkerScale;
                        TempPlacemark.name = "placemark";

                        //TempPlacemark.transform.position = new Vector3((float)unityPosition.z, (float)unityPosition.x, (float)unityPosition.y); // this will need to account for offset
                        TempPlacemark.transform.position = new Vector3((float)x, (float)y, (float)z); // this will need to account for offset
                        TempPlacemark.transform.rotation = new Quaternion((float)rot_x, (float)rot_y, (float)rot_z, (float)rot_w); // this will need to account for offset
                        placemarks.Add(TempPlacemark);


                        //do calculations
                        Vector3Decimal realPosition = VirtualMeter.CalculateRealPositionOfPoint(placemarks[0].transform.position - 40 * Vector3.up);

                        // create value ditionary
                        Dictionary<string, decimal> dict = new Dictionary<string, decimal>()
                    {
                        { "Lat(°)" ,    realPosition.z },
                        { "Lon(°)" ,     realPosition.x },
                        { "Altitude(m)" ,    realPosition.y }
                    };

                        DateTime myDate = DateTime.ParseExact(date, "MM/dd/yyyy H:mm:ss.ffff",
                                           System.Globalization.CultureInfo.InvariantCulture);

                        // create tool instance
                        toolControllerComponent.CreateToolInstance("Placemark", "", comment,
                            Tool.toolType.PLACEMARK,
                            dict, placemarks, myDate, false, false, true, localID, data[1], false);




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

        string FilePath = "Outputs/Placemarks";
        

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

        var path = Path.Combine(directoryPath, string.Format("{0}.csv", instance.ToolTitle.ToString() + " " + DateTime.Now.ToString("yyyyMMdd_Hmmssffff") ));

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        //var sr = File.CreateText(path);
        StreamWriter writer = new StreamWriter(path, true);

        string csvData = "";
        csvData += "#Index, Id, Lat, Lon, z, unity_z, unity_x,unity_y, unity_rotation_z, unity_rotation_x, unity_rotation_y,unity_rotation_w, Date, Comments";
        writer.WriteLine(csvData, "en-GB");

        Vector3Decimal realPosition = new Vector3Decimal();
        for (int i = 0; i < instance.PlacemarkList.Count; ++i)
        {
            realPosition = VirtualMeter.CalculateGPSPosition(instance.PlacemarkList[i].transform.position);


            csvData = i.ToString() + ",";
            csvData += instance.ID;//instance.creationDate.ToString("yyyy.MMddHmmssffff")+",";
            csvData += realPosition.z.ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
            csvData += realPosition.x.ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
            csvData += realPosition.y.ToString("0.000", new CultureInfo("en-GB")) + ",";
            csvData += instance.PlacemarkList[i].transform.position.z.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.PlacemarkList[i].transform.position.x.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.PlacemarkList[i].transform.position.y.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.PlacemarkList[i].transform.rotation.z.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.PlacemarkList[i].transform.rotation.x.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.PlacemarkList[i].transform.rotation.y.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.PlacemarkList[i].transform.rotation.w.ToString("0.0000", new CultureInfo("en-GB")) + ",";


            csvData += instance.creationDate.ToString("MM/dd/yyyy H:mm:ss.ffff") + ",";
            //csvData += DateTime.Now.ToString("yyyyMMdd_Hmmssffff") + ",";
            csvData += instance.CustomTxt;
            writer.WriteLine(csvData, "en-GB");

        }

       //writer.WriteLine(csvData, "en-GB");
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
        realPosition = VirtualMeter.CalculateGPSPosition(instance.PlacemarkList[0].transform.position);
        var point = new Point();
        point.Coordinate = new Vector((double)realPosition.z, (double)realPosition.x, (double)realPosition.y);
        
        // This is the Element to save to the Kml file.
        var placemark = new Placemark();
        placemark.Geometry = point;
        placemark.Name = instance.ToolTitle;
        placemark.Description =  new Description() { Text = instance.CustomTxt };
        placemark.StyleUrl = new Uri("#m_ylw-pushpin", UriKind.Relative);

        // create the document
        var document = new SharpKml.Dom.Document();
        document.Name = instance.ToolTitle + " " + instance.ID;
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

       // ShowNotification("Done-dopne-dome!", 1.5f);
       // ToolController.globalToolControllerObject.StartCoroutine(Tool.ShowNotification("Done!", 1.5f));
    }

    public void SaveMultiInstance()
    {
        if (instanceList.Count == 0)
        {
            ToolController.globalToolControllerObject.StartCoroutine(Tool.ShowNotificationLabelForMesuring("No data!", 1.5f));
        }
        else
        {
            string FilePath = "Outputs/Placemarks";


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

            var path = Path.Combine(directoryPath, "AllPlacemarks " + DateTime.Now.ToString("yyyyMMdd_Hmmssffff") + ".csv");

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var sr = File.CreateText(path);
            string csvData = "";
            csvData += "#Index, Id, Lat, Lon, z, unity_z, unity_x,unity_y, unity_rotation_z, unity_rotation_x, unity_rotation_y,unity_rotation_w, Date, Comments";

            sr.WriteLine(csvData);
            int j = 0;
            Vector3Decimal realPosition = new Vector3Decimal();
            foreach (var p in instanceList)
            {
                for (int i = 0; i < p.GetComponent<ToolInstance>().PlacemarkList.Count; ++i)
                {
                    realPosition = VirtualMeter.CalculateGPSPosition(p.GetComponent<ToolInstance>().PlacemarkList[i].transform.position);
                    // csvData += i.ToString() + ",";
                    csvData = j.ToString() + ",";
                    csvData += p.GetComponent<ToolInstance>().ID;//creationDate.ToString("yyyy.MMddHmmssffff") + ",";
                    csvData += ((double)realPosition.z).ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                    csvData += ((double)realPosition.x).ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                    csvData += ((double)realPosition.y).ToString("0.000", new CultureInfo("en-GB")) + ",";

                    csvData += p.GetComponent<ToolInstance>().PlacemarkList[i].transform.position.z.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += p.GetComponent<ToolInstance>().PlacemarkList[i].transform.position.x.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += p.GetComponent<ToolInstance>().PlacemarkList[i].transform.position.y.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += p.GetComponent<ToolInstance>().PlacemarkList[i].transform.rotation.z.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += p.GetComponent<ToolInstance>().PlacemarkList[i].transform.rotation.x.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += p.GetComponent<ToolInstance>().PlacemarkList[i].transform.rotation.y.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += p.GetComponent<ToolInstance>().PlacemarkList[i].transform.rotation.w.ToString("0.0000", new CultureInfo("en-GB")) + ",";



                    csvData += p.GetComponent<ToolInstance>().creationDate.ToString("MM/dd/yyyy H:mm:ss.ffff") + ",";
                    csvData += p.GetComponent<ToolInstance>().CustomTxt;// + "\n";
                    sr.WriteLine(csvData);
                }
                j++;
            }

            // sr.WriteLine(csvData);
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
            document.Name = "AllPlacemarks " + DateTime.Now.ToString("yyyyMMdd_Hmmssffff");
            document.AddStyle(stylemap);
            document.AddStyle(style);
            document.AddStyle(style2);

            // This will be the location of the Placemark.
            foreach (var p in instanceList)
            {
                realPosition = VirtualMeter.CalculateGPSPosition(p.GetComponent<ToolInstance>().PlacemarkList[0].transform.position);
                var point = new Point();
                point.Coordinate = new Vector((double)realPosition.z, (double)realPosition.x, (double)realPosition.y);

                // This is the Element to save to the Kml file.
                var placemark = new Placemark();
                placemark.Geometry = point;
                placemark.Name = p.GetComponent<ToolInstance>().ToolTitle + " " + p.GetComponent<ToolInstance>().ID;//creationDate.ToString("yyyy.MMddHmmssffff");
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


            // ToolController.globalToolControllerObject.StartCoroutine(Tool.ShowNotification("Multiple measure has been exported", 1));
            ToolController.globalToolControllerObject.StartCoroutine(Tool.ShowNotificationLabelForMesuring("Done!", 1.5f));
        }
    }

    public void DeleteAllInstances()
    {

        foreach (var p in instanceList)
        {
            for (int i = 0; i < p.GetComponent<ToolInstance>().PlacemarkList.Count; ++i)
            {
           
                p.GetComponent<ToolInstance>().DestroySingleInstance();
            }

            localID--;

        }
        instanceList.Clear();
        PlacemarkMap.Clear();

    }


    public void ShowHide()
    {

        bool status = true;
        if (instanceList.Count>0)
            status = instanceList[0].GetComponent<ToolInstance>().PlacemarkList[0].activeSelf;

        foreach (var p in instanceList)
        {
            for (int i = 0; i < p.GetComponent<ToolInstance>().PlacemarkList.Count; ++i)
            {
                             
                p.GetComponent<ToolInstance>().PlacemarkList[i].SetActive(!status);
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
            statusString = instanceList[0].GetComponent<ToolInstance>().PlacemarkList[0].activeSelf ? "Hide" : "Show";

        GameObject.Find("Canvas").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = statusString;

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = statusString;

    }


}
