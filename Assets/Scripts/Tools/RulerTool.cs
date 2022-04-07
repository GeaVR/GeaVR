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
using UnityEngine.UI;
using System.IO;
using System;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System.Globalization;
using System.Text.RegularExpressions;

public class RulerTool : Tool
{
    public static int localID = 0;

    public static Dictionary<string, decimal> dictionary;

    [HideInInspector]
    public static List<GameObject> instanceList = new List<GameObject>();
    [HideInInspector]
    public static List<string> RulerMap = new List<string>();

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
        // hold until trigger is released
        // this avoids instant placement 
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
        {
            while (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.2f)
            {
                yield return wfeof;
            }
        }

        // temporary objects
        GameObject TempPlacemark = null, TempPlacemarkInfo = null;
        List<GameObject> placemarks = new List<GameObject>();
        Vector3Decimal realPosition;

        // Place 2 markers
        for (int times = 0; times < 2; times++)
        {
            //Instantiate placemark
            TempPlacemark = Instantiate(toolControllerComponent.PlacemarkObject3);
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
                if (checkIfToolShouldQuit())
                {
                 

                    placemarkConfirmed = true;
                    Destroy(TempPlacemark);
                    foreach (var p in placemarks)
                    {
                        Destroy(p);
                    }
                    ToolController.ToolIsCurrentlyRunning = false;
                    ToolController.ToolControllerInterfaceIsCurrentlyRunning = true;
                    toolControllerComponent.OculusMasterObject.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.2783019f, 0.3061422f, 1, 1);

                    toolControllerComponent.MeasurementControlUI.gameObject.SetActive(true);
                    if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
                        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.SetActive(true);

                    yield break; // end coroutine early 
                }

                if (StateSingleton.stateView != StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView != StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
                {
                    if (wasHolding == true && OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) < 0.2f)
                        wasHolding = false;
                }
                //Trailing Waypoint
                if (Physics.Raycast(master.transform.position, directionMaster.transform.forward, out hit, 100000))
                {
                      TempPlacemark.transform.position = hit.point;
                    TempPlacemark.transform.eulerAngles = new Vector3(0f, directionMaster.transform.eulerAngles.y, directionMaster.transform.eulerAngles.z);
                    legalPlacemarkPlace = true;

                    //Showing info on toolInfo
                    realPosition = VirtualMeter.CalculateGPSPosition(TempPlacemark.transform.position);
                  	toolControllerComponent.updateGUIMenus(
                        ((times == 0) ? "First " : "Second ") + "Placemark",
                        "Northing (m): " + realPosition.z.ToString("0.000") +
                        "\nEasting (m): " + realPosition.x.ToString("0.000") +
                        "\nAltitude (m): " + realPosition.y.ToString("0.000")
                        );

                }
                else
                {
                    //If not well placed then invalidate
                    TempPlacemark.transform.position = master.transform.position + directionMaster.transform.forward * 100000;
                    toolControllerComponent.updateGUIMenus(
                        ((times == 0) ? "First " : "Second ") + "Placemark", 
                        "Northing (m): undefined \nEasting (m): undefined \nAltitude (m): undefined");
                    legalPlacemarkPlace = false;                    
                }
                                
                if (
                (
                Input.GetMouseButton(0) ||
                    (
                        (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS) &&
                        OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.5f && wasHolding == false
                    )
                )
                && legalPlacemarkPlace)
                {                    
                    placemarkConfirmed = true;
                    placemarks.Add(TempPlacemark);
                    //ToolController.ToolIsCurrentlyRunning = false;

                }
                yield return wfeof;
            }
            while (Input.GetMouseButton(0))
                yield return wfeof;
        }

        if (placemarks.Count == 2)
        {
            localID++;
            // create line renderer
            LineRenderer lr = TempPlacemark.AddComponent<LineRenderer>() as LineRenderer;
            lr.startWidth = lr.endWidth = (float)(toolControllerComponent.MarkerScale * 0.5f);
            lr.material = toolControllerComponent.MeasurementMaterial;
            lr.SetPosition(0, placemarks[0].transform.position + Vector3.up * (float)(toolControllerComponent.MarkerScale * 0.5f));
            lr.SetPosition(1, placemarks[1].transform.position + Vector3.up * (float)(toolControllerComponent.MarkerScale * 0.5f));


            //do calculations
            Vector3 uPos1 = placemarks[1].transform.position;
            Vector3 uPos2 = placemarks[0].transform.position;

            decimal profileDistance = PositionController.CalculateRealDistance(uPos1, uPos2);
            decimal hDistance = PositionController.CalculateRealDistance2D(uPos1, uPos2);
            decimal vDistance = PositionController.CalculateVerticalRealDistance2D(uPos1, uPos2);
            decimal GroundDistance = PositionController.CalculateGroundDistance(uPos1, uPos2, lr);
            float heading = PositionController.CalculateHeadingFromPositions(uPos1, uPos2);
            float inclination = PositionController.CalculateInclinationFromPositions(uPos1, uPos2);
            toolControllerComponent.OculusMasterObject.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.2783019f, 0.3061422f, 1, 1);

            dictionary = new Dictionary<string, decimal>();
            dictionary.Add("Line_length", profileDistance);
            dictionary.Add("hDistance", hDistance);
            dictionary.Add("vDistance", vDistance);
            dictionary.Add("Ground_distance", GroundDistance);
            dictionary.Add("Heading", (decimal)heading);
            dictionary.Add("Inclination", (decimal)inclination);

            oldNote = "\n3D Line Length(m): " + profileDistance.ToString("0.000") +
                "\nH Line Length (m): " + hDistance.ToString("0.000") +
                "\nV Line Length (m): " + vDistance.ToString("0.000") +
                "\nHeading (°): " + heading.ToString("0.0") +
                "\nInclination (°): " + inclination.ToString("0.0")+"\nNote: ";


            // create value dictionary 
            // create tool instance        
            DateTime date = DateTime.Now;
            string globalID = date.ToString("yyyy.MMddHmmssffff") + ",";

            toolControllerComponent.CreateToolInstance("Ruler", oldNote
               , "",
				Tool.toolType.RULER,
                dictionary, placemarks, DateTime.Now, true, false, true, localID,globalID);
          
        }
        else
        {
            foreach (var p in placemarks)
            {
                Destroy(p);
            }
            ToolController.ToolIsCurrentlyRunning = false;
        }
    }

    public void startToolInterface()
    {

        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("UpperPanel").gameObject.transform.Find("Text").gameObject.GetComponent<Text>().text = "Ruler tool";
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_line").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_ruler").gameObject.SetActive(true);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_surface").gameObject.SetActive(false);

        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_line").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_surface").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_ruler").gameObject.SetActive(true);


        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_line").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_surface").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_ruler").gameObject.SetActive(true);

        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_line").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_ruler").gameObject.SetActive(true);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_surface").gameObject.SetActive(false);

        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_ruler").gameObject.SetActive(true);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_surface").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_line").gameObject.SetActive(false);


        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_placemark").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_line").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_polygon").gameObject.SetActive(false);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_ruler").gameObject.SetActive(true);
        toolControllerComponent.MeasurementControlUI.gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_surface").gameObject.SetActive(false);


        GameObject.Find("Canvas").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.Find("Canvas").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.AddListener(GetInsertedNotes);


        toolControllerComponent.MeasurementControlUI.SetActive(true);
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("UpperPanel").gameObject.transform.Find("Text").gameObject.GetComponent<Text>().text = "Ruler tool";
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.SetActive(true);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_ruler").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StartTool_surface").gameObject.SetActive(false);


            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_surface").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("StopTool_ruler").gameObject.SetActive(true);


            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_surface").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_ruler").gameObject.SetActive(true);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_ruler").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("SaveToMemory_surface").gameObject.SetActive(false);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_ruler").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_surface").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("EmptyTrash_polygon").gameObject.SetActive(false);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_ruler").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("ShowHide_surface").gameObject.SetActive(false);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_placemark").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_line").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_polygon").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_ruler").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.transform.Find("Content").gameObject.transform.Find("OpenFromMemory_surface").gameObject.SetActive(false);


            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").transform.localPosition = new Vector3(0.0f, -400.0f, 0.0f);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.AddListener(GetInsertedNotes);


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
        var path = Path.Combine(directoryPath, string.Format("rulers.csv"));
        LoadFromFile(path.ToString());
        ToolController.globalToolControllerObject.StartCoroutine(Tool.ShowNotificationLabelForMesuring("Done!", 1.5f));

    }


    public override GameObject LoadFromFile(string FilePath)
    {

        if (File.Exists(FilePath))
        {

            String fileData = System.IO.File.ReadAllText(FilePath);
            String[] lines = fileData.Split("\n"[0]);
            GameObject TempPlacemark =null;

            foreach (String line in lines)
            {
                Debug.Log("line: " + line);
                String[] data = line.Split(","[0]);

                if (data.Length == 30 && data[0][0].ToString() != "#") // discard non-coordinate lines
                {

                    if (!RulerMap.Contains(data[1]))
                    {
                        List<GameObject> placemarks = new List<GameObject>();

                        decimal [] z = { decimal.Parse(data[5], NumberStyles.Any, new CultureInfo("en-GB")), decimal.Parse(data[15],  NumberStyles.Any, new CultureInfo("en-GB")) };
                        decimal [] x = { decimal.Parse(data[6], NumberStyles.Any, new CultureInfo("en-GB")), decimal.Parse(data[16], NumberStyles.Any, new CultureInfo("en-GB")) };
                        decimal [] y = { decimal.Parse(data[7], NumberStyles.Any, new CultureInfo("en-GB")), decimal.Parse(data[17], NumberStyles.Any, new CultureInfo("en-GB")) };

                        decimal [] rot_z = { decimal.Parse(data[8], NumberStyles.Any, new CultureInfo("en-GB")), decimal.Parse(data[18], NumberStyles.Any, new CultureInfo("en-GB")) };
                        decimal [] rot_x = {decimal.Parse(data[9],  NumberStyles.Any, new CultureInfo("en-GB")), decimal.Parse(data[19], NumberStyles.Any, new CultureInfo("en-GB")) };
                        decimal [] rot_y = {decimal.Parse(data[10],  NumberStyles.Any, new CultureInfo("en-GB")), decimal.Parse(data[20], NumberStyles.Any, new CultureInfo("en-GB")) };
                        decimal [] rot_w = {decimal.Parse(data[11], NumberStyles.Any, new CultureInfo("en-GB")), decimal.Parse(data[21], NumberStyles.Any, new CultureInfo("en-GB")) };

                        string date = data[28];
                        string comment = Regex.Replace(data[29], @"\t|\n|\r", "");


                        // Place 2 markers
                        for (int times = 0; times < 2; times++)
                        {
                            //Instantiate placemark
                            TempPlacemark = Instantiate(toolControllerComponent.PlacemarkObject3);
                            TempPlacemark.transform.localScale *= toolControllerComponent.MarkerScale;
                            TempPlacemark.name = "distance placemark";

                            TempPlacemark.transform.position = new Vector3((float)x[times], (float)y[times], (float)z[times]); // this will need to account for offset
                            TempPlacemark.transform.rotation = new Quaternion((float)rot_x[times], (float)rot_y[times], (float)rot_z[times], (float)rot_w[times]); // this will need to account for offset
                            placemarks.Add(TempPlacemark);
                        }

                            LineRenderer lr = TempPlacemark.AddComponent<LineRenderer>() as LineRenderer;
                            lr.startWidth = lr.endWidth = (float)(toolControllerComponent.MarkerScale * 0.5f);
                            lr.material = toolControllerComponent.MeasurementMaterial;
                            lr.SetPosition(0, placemarks[0].transform.position + Vector3.up * (float)(toolControllerComponent.MarkerScale * 0.5f));
                            lr.SetPosition(1, placemarks[1].transform.position + Vector3.up * (float)(toolControllerComponent.MarkerScale * 0.5f));

                            dictionary = new Dictionary<string, decimal>();
                            dictionary.Add("Line_length", (decimal)decimal.Parse(data[22], NumberStyles.Any, new CultureInfo("en-GB")));
                            dictionary.Add("Ground_distance", (decimal)decimal.Parse(data[23], NumberStyles.Any, new CultureInfo("en-GB")));
                            dictionary.Add("hDistance", (decimal)decimal.Parse(data[24], NumberStyles.Any, new CultureInfo("en-GB")));
                            dictionary.Add("vDistance", (decimal)decimal.Parse(data[25], NumberStyles.Any, new CultureInfo("en-GB")));
                            dictionary.Add("Heading", (decimal)decimal.Parse(data[24], NumberStyles.Any, new CultureInfo("en-GB")));
                            dictionary.Add("Inclination", (decimal)decimal.Parse(data[25], NumberStyles.Any, new CultureInfo("en-GB")));

                            oldNote = "\nLine Length(m): " + decimal.Parse(data[22], NumberStyles.Any, new CultureInfo("en-GB")).ToString("0.000") +
                                  "\nH Line Length(m) (m): " + decimal.Parse(data[24], NumberStyles.Any, new CultureInfo("en-GB")).ToString("0.000")+
                                 "\nV Line Length(m) (m): " + decimal.Parse(data[25], NumberStyles.Any, new CultureInfo("en-GB")).ToString("0.000")+
                                "\nHeading (°): " + decimal.Parse(data[26], NumberStyles.Any, new CultureInfo("en-GB")).ToString("0.0") +
                                "\nInclination (°): " + decimal.Parse(data[27], NumberStyles.Any, new CultureInfo("en-GB")).ToString("0.0") + "\nNote: ";

    
                            // create value dictionary 
                            // create tool instance        

                            DateTime myDate = DateTime.ParseExact(date, "MM/dd/yyyy H:mm:ss.ffff",
                                       System.Globalization.CultureInfo.InvariantCulture);

                            // create tool instance
                            toolControllerComponent.CreateToolInstance("Ruler", "", comment,
                                Tool.toolType.RULER,
                                dictionary, placemarks, myDate, true, false, true, localID, data[1], false);

                        localID++;

                    }
                    else
                    {
                        Debug.Log("Already existing");

                    }
                }
            }
        }
        return null;
    }


    public void DeleteAllInstances()
    {

        foreach (var p in instanceList)
        {

            for (int i = 0; i < p.GetComponent<ToolInstance>().RulerList.Count; ++i)
            {
                p.GetComponent<ToolInstance>().DestroySingleInstance();
            }

            localID--;

        }
        instanceList.Clear();
        RulerMap.Clear();

    }

    public static void SaveSingleInstance(ToolInstance instance)
    {
        string FilePath = "Outputs/Ruler";


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
        csvData += "#Index, Id, Lat1, Lon1, z1, unity_z1, unity_x1, unity_y1, unity_rotation_z1, unity_rotation_x1, unity_rotation_y1,unity_rotation_w1, Lat2, Lon2, z2, unity_z2, unity_x2, unity_y2, unity_rotation_z2, unity_rotation_x2, unity_rotation_y2, unity_rotation_w2, Line_length, Ground_distance, H Distance, V Distance,  Heading, Inclination, Date, Comments";
        writer.WriteLine(csvData, "en-GB");
        Vector3Decimal realPosition = new Vector3Decimal();
        int j = 1;
        for (int i = 0; i < instance.RulerList.Count; ++i)
        {
        
            realPosition = VirtualMeter.CalculateGPSPosition(instance.RulerList[i].transform.position);

            if (j % 2 != 0)
            {
                csvData = (i / 2).ToString() + ",";
                csvData += instance.ID;
            }

            csvData += realPosition.z.ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
            csvData += realPosition.x.ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
            csvData += realPosition.y.ToString("0.000", new CultureInfo("en-GB")) + ",";

            
            csvData += instance.RulerList[i].transform.position.z.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.RulerList[i].transform.position.x.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.RulerList[i].transform.position.y.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.RulerList[i].transform.rotation.z.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.RulerList[i].transform.rotation.x.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.RulerList[i].transform.rotation.y.ToString("0.0000", new CultureInfo("en-GB")) + ",";
            csvData += instance.RulerList[i].transform.rotation.w.ToString("0.0000", new CultureInfo("en-GB")) + ",";

            if (j % 2 == 0)
            {

                csvData += dictionary["Line_length"].ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                csvData += dictionary["Ground_distance"].ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                csvData += dictionary["hDistance"].ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                csvData += dictionary["vDistance"].ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                csvData += dictionary["Heading"].ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                csvData += dictionary["Inclination"].ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
          

                csvData += instance.creationDate.ToString("MM/dd/yyyy H:mm:ss.ffff") + ",";
                csvData += instance.CustomTxt ;
                writer.WriteLine(csvData, "en-GB");
            }
            j++;
        }

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

        foreach (var pl in instance.GetComponent<ToolInstance>().RulerList)
        {
            realPosition = VirtualMeter.CalculateGPSPosition(pl.transform.position);
            Vector linePoint = new Vector((double)realPosition.z, (double)realPosition.x, (double)realPosition.y);
            line.Coordinates.Add(linePoint);
        }

        var placemark = new Placemark();
        placemark.Geometry = line;
        placemark.Name = instance.ToolTitle;
        placemark.StyleUrl = new Uri("#m_ylw-pushpin", UriKind.Relative);

        // create the document
        var document = new SharpKml.Dom.Document();
        document.Name = instance.ToolTitle + " " + instance.ID;//creationDate.ToString("yyyy.MMddHmmssffff");
        placemark.Description = new Description() { Text = instance.CustomTxt };
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
    }

    public void SaveMultiInstance()
    {
        if (instanceList.Count == 0)
        {
            ToolController.globalToolControllerObject.StartCoroutine(Tool.ShowNotificationLabelForMesuring("No data!", 1.5f));
        }
        else
        {
            string FilePath = "Outputs/Ruler";

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

            var path = Path.Combine(directoryPath, "AllRulers " + DateTime.Now.ToString("yyyyMMdd_Hmmssffff") + ".csv");

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var sr = File.CreateText(path);
            string csvData = "";
            csvData += "#Index, Id, Lat1, Lon1, z1, unity_z1, unity_x1, unity_y1, unity_rotation_z1, unity_rotation_x1, unity_rotation_y1,unity_rotation_w1, Lat2, Lon2, z2, unity_z2, unity_x2, unity_y2, unity_rotation_z2, unity_rotation_x2, unity_rotation_y2, unity_rotation_w2, Line_length, Ground_distance, H Distance, V Distance,  Heading, Inclination, Date, Comments";
            sr.WriteLine(csvData, "en-GB");

            Vector3Decimal realPosition = new Vector3Decimal();
            int z = 0;
            foreach (var inst in instanceList)
            {

                int j = 1;
                for (int i = 0; i < inst.GetComponent<ToolInstance>().RulerList.Count; ++i)
                {
                    realPosition = VirtualMeter.CalculateGPSPosition(inst.GetComponent<ToolInstance>().RulerList[i].transform.position);

                    if (j % 2 != 0)
                    {
                        csvData = z.ToString() + ",";
                        csvData += inst.GetComponent<ToolInstance>().ID;
                        z++;
                    }

                    csvData += realPosition.z.ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                    csvData += realPosition.x.ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                    csvData += realPosition.y.ToString("0.000", new CultureInfo("en-GB")) + ",";

                    csvData += inst.GetComponent<ToolInstance>().RulerList[i].transform.position.z.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += inst.GetComponent<ToolInstance>().RulerList[i].transform.position.x.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += inst.GetComponent<ToolInstance>().RulerList[i].transform.position.y.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += inst.GetComponent<ToolInstance>().RulerList[i].transform.rotation.z.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += inst.GetComponent<ToolInstance>().RulerList[i].transform.rotation.x.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += inst.GetComponent<ToolInstance>().RulerList[i].transform.rotation.y.ToString("0.0000", new CultureInfo("en-GB")) + ",";
                    csvData += inst.GetComponent<ToolInstance>().RulerList[i].transform.rotation.w.ToString("0.0000", new CultureInfo("en-GB")) + ",";


                    if (j % 2 == 0)
                    {
                        csvData += inst.GetComponent<ToolInstance>().ValueDict["Line_length"].ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                        csvData += inst.GetComponent<ToolInstance>().ValueDict["Ground_distance"].ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                        csvData += inst.GetComponent<ToolInstance>().ValueDict["hDistance"].ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                        csvData += inst.GetComponent<ToolInstance>().ValueDict["vDistance"].ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";

                        csvData += inst.GetComponent<ToolInstance>().ValueDict["Heading"].ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                        csvData += inst.GetComponent<ToolInstance>().ValueDict["Inclination"].ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
                   
                        csvData += inst.GetComponent<ToolInstance>().creationDate.ToString("MM/dd/yyyy H:mm:ss.ffff") + ",";
                        csvData += inst.GetComponent<ToolInstance>().CustomTxt;

                        sr.WriteLine(csvData, "en-GB");
                    }
                    j++;
                }
            }

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
            document.Name = "AllRulers " + DateTime.Now.ToString("yyyyMMdd_Hmmssffff");
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

                foreach (var pl in inst.GetComponent<ToolInstance>().RulerList)
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


    public void ShowHide()
    {

        bool status = true;
        if (instanceList.Count > 0)
            status = instanceList[0].GetComponent<ToolInstance>().RulerList[0].activeSelf;

        foreach (var p in instanceList)
        {
            for (int i = 0; i < p.GetComponent<ToolInstance>().RulerList.Count; ++i)
            {


                p.GetComponent<ToolInstance>().RulerList[i].SetActive(!status);
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

    }

    public void OnPointerEnter()
    {
        string statusString = "Hide / Show";
        if (instanceList.Count > 0)
            statusString = instanceList[0].GetComponent<ToolInstance>().RulerList[0].activeSelf ? "Hide" : "Show";

        GameObject.Find("Canvas").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = statusString;

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = statusString;
    }

}
