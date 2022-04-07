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

using UnityStandardAssets.Characters.FirstPerson;

using System.IO;
using System;


public class PhotoEntry
{
    public String Id { get; set; }
    public String Path { get; set; }
    public String PhotoName { get; set; }
    public String MetadataName { get; set; }
    public String Lat { get; set; }
    public String Lon { get; set; }
    public String Z { get; set; }
    public String Note { get; set; }


}

public class ToolController : MonoBehaviour {

    //
    private List<GameObject> ToolInstances;

    // inspector interface 
    public float MarkerScale = 1f;
    public float WorldUIScale = 0.2f;

    public GameObject PlacemarkObject;
    public GameObject ToolMenuPrefab;
    public GameObject ToolGraphPrefab;
    public GameObject NotesMenuPrefab;

    public GameObject ToolInstancePrefab;

    public GameObject WalkingMasterObject;
    public GameObject FlyingMasterObject;
    public GameObject DroneMasterObject;
    public GameObject OculusMasterObject, OculusDirectionMasterObject;

    public GameObject ViewControllerGameObject;
    public GameObject VirtualMeterGameObject;

    public GameObject ToolInfoMenu = null;
 
    [HideInInspector]
    public GameObject OculusCanvas = null;
    [HideInInspector]
    public GameObject OculusToolInfoMenu = null;
    [HideInInspector]
    public GameObject OculusHandToolInfoMenu = null;
    [HideInInspector]
    public static ToolController globalToolControllerObject = null;

    public GameObject PlacemarkScrollView = null;

    public GameObject TopographicGraph = null;


    [HideInInspector]
    public GameObject OculusTopographicGraph = null;

    [HideInInspector]
    public static bool ToolIsCurrentlyRunning = false;
    public static bool ToolControllerInterfaceIsCurrentlyRunning = false;
    public Material LineMaterial;

    //[HideInInspector]
    //public bool ToolIsCurrentlyRunning = false;

    // added MK 06/01/19
    public Material PolyMaterial;
	public Material MeasurementMaterial;	
    public GameObject PlacemarkObject2;
    public GameObject PlacemarkObject3;
    public GameObject SurfaceToolObject;

    //addedd FV
    public GameObject ToolMenuInstance;

    public GameObject CameraControlUI = null;
    //  [HideInInspector]
    public GameObject OculusCameraControlUI = null;

    //  [HideInInspector]
    public Sprite spriteImageInitial = null;


    public GameObject MeasurementControlUI = null;
    public GameObject GpsTrackControlUI = null;
    public GameObject GpsStopControlButton = null;
    public GameObject GpsStartControlButton = null;
    public string WaypointNameforSession = "";
    public string PhotoDatabaseFile = "";
    public GameObject WaypointMenu = null;
    public GameObject WaypointIdCounter = null;
    public GameObject WaypointCorrdinatesText = null;
    public GameObject WaypointText = null;
    public GameObject WaypointNote = null;
    public GameObject WaypointPicture = null;
    public GameObject WaypointPictureImage = null;

    public GameObject NotificationText = null;
    public GameObject NotificationPanel = null;

    public GameObject LocationOnMapTool = null;
    public GameObject NotesMenu = null;

    public GameObject[] hands = null;
    [HideInInspector]
    public List<PhotoEntry> ListPhoto;
    [HideInInspector]
    public int actualPhoto;
    public void Start()
    {
       
        var directory = new DirectoryInfo(Application.dataPath);
        var directoryPath = Path.Combine(directory.Parent.FullName, "Outputs/Waypoints");
        var directoryPathCamera = Path.Combine(directory.Parent.FullName, "Outputs/Camera");
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            if (!Directory.Exists(directoryPathCamera))
            {
                Directory.CreateDirectory(directoryPathCamera);
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
        }

        //var path_csv = Path.Combine(directoryPath, string.Format("wp_{0}.csv", DateTime.Now.ToString("yyyyMMdd_Hmmssffff")));
        WaypointNameforSession = Path.Combine(directoryPath, string.Format("wp_{0}.csv", DateTime.Now.ToString("yyyyMMdd_Hmmssffff")));
        PhotoDatabaseFile = Path.Combine(directoryPathCamera , string.Format("photoDb_{0}.csv", DateTime.Now.ToString("yyyyMMdd_Hmmssffff")));

        ListPhoto = new List<PhotoEntry>();


        globalToolControllerObject = this;

    }

    public void GetOculusCanvas()
    {
            OculusCanvas= ViewControllerGameObject.GetComponent<ViewController>().CanvasOculus;

    }

    // get controlling objects
    public Transform GetMaster()
    {
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
        {
            return OculusMasterObject.transform;
        }
        switch (StateSingleton.stateMode)
        {
            case StateSingleton.StateMode.WALKING:
                return WalkingMasterObject.transform;
                break;
            case StateSingleton.StateMode.FLIGHT:
                return FlyingMasterObject.transform;
                break;
            case StateSingleton.StateMode.DRONE:
                return DroneMasterObject.transform;
                break;
       }
       return transform;    
    }

    public Transform GetDirectionMaster()
    {
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
        {
            return OculusDirectionMasterObject.transform;
        }
        switch (StateSingleton.stateMode)
        {
            case StateSingleton.StateMode.WALKING:
                return WalkingMasterObject.GetComponentInChildren<Camera>().transform;
                break;
            case StateSingleton.StateMode.FLIGHT:
                return FlyingMasterObject.GetComponentInChildren<Camera>().transform;
                break;
            case StateSingleton.StateMode.DRONE:
                return DroneMasterObject.GetComponentInChildren<Camera>().transform;
                break;
        }        
        return transform;
    }

    // the following shouldStopPlacing be refactored, probably removed from this object

    public void updateGUIMenus(string _Title, string _Text, string _Notes = "")
    {
        if (ToolInfoMenu)
        {
            ToolInfoMenu.GetComponentsInChildren<Text>(true)[0].text = _Title;
            ToolInfoMenu.GetComponentsInChildren<Text>(true)[1].text = _Text;
            ToolInfoMenu.GetComponentsInChildren<Text>(true)[2].text = _Notes;
        }
        if (OculusToolInfoMenu)
        {
            OculusToolInfoMenu.GetComponentsInChildren<Text>(true)[0].text = _Title;
            OculusToolInfoMenu.GetComponentsInChildren<Text>(true)[1].text = _Text;
			OculusToolInfoMenu.GetComponentsInChildren<Text>(true)[2].text = _Notes;
        }
        if (OculusHandToolInfoMenu)
        {
            OculusHandToolInfoMenu.GetComponentsInChildren<Text>(true)[0].text = _Title;
            OculusHandToolInfoMenu.GetComponentsInChildren<Text>(true)[1].text = _Text;
        }
        if (TopographicGraph)
        {
            TopographicGraph.GetComponentsInChildren<Text>(true)[0].text = _Title;
            TopographicGraph.GetComponentsInChildren<Text>(true)[1].text = _Text;
            TopographicGraph.GetComponentsInChildren<Text>(true)[2].text = _Notes;
        }
        if (OculusTopographicGraph)
        {
            OculusTopographicGraph.GetComponentsInChildren<Text>(true)[0].text = _Title;
            OculusTopographicGraph.GetComponentsInChildren<Text>(true)[1].text = _Text;
            OculusTopographicGraph.GetComponentsInChildren<Text>(true)[2].text = _Notes;
        }
    }

    public GameObject CreateToolInstance(string _Title, string _Text, string _Desc, Tool.toolType _ToolType, 
        Dictionary<string, decimal> Values, List<GameObject> Placemarks, DateTime creationDate,
        bool DestroyOnClose = false, bool hasGraph = false, bool ShowWorldMenu = true, int localID=0, string globalID= "", bool showToolSummaryUI = true)
    {
        //ToolInfoMenu.SetActive(!hasGraph);
        //TopographicGraph.SetActive(hasGraph);




        //update oculus menu
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
        {
            OculusCanvas = ViewControllerGameObject.GetComponent<ViewController>().CanvasOculus;
            OculusToolInfoMenu = ViewControllerGameObject.GetComponent<ViewController>().CanvasOculus.transform.Find("Current Tool").gameObject;
            OculusTopographicGraph = ViewControllerGameObject.GetComponent<ViewController>().CanvasOculus.transform.Find("Current Graph").gameObject;
            //CameraControlUI = ViewControllerGameObject.GetComponent<ViewController>().CanvasOculus.transform.Find("CameraControlUI").gameObject; ;
            //OculusToolInfoMenu.SetActive(!hasGraph);
            //OculusHandToolInfoMenu.SetActive(!hasGraph);
            //OculusTopographicGraph.SetActive(hasGraph);
        }

        GameObject NewToolInstance = null;
        NewToolInstance = (GameObject)Instantiate(ToolInstancePrefab);
        NewToolInstance.GetComponent<ToolInstance>().PostStart(this, _Title, _Text, _Desc, _ToolType, Values, Placemarks, creationDate, DestroyOnClose, hasGraph, ShowWorldMenu, localID, globalID, showToolSummaryUI);

        switch (NewToolInstance.GetComponent<ToolInstance>().instanceToolType)
        {
            case Tool.toolType.PLACEMARK:
                PlacemarkTool.instanceList.Add(NewToolInstance);
                PlacemarkTool.PlacemarkMap.Add(globalID);
                break;

            case Tool.toolType.POLYGON:
                PolygonTool.instanceList.Add(NewToolInstance);
                PolygonTool.PolygonMap.Add(globalID);
                break;

            case Tool.toolType.LINE:
               
                PolylineTool.instanceList.Add(NewToolInstance);
                PolylineTool.LineMap.Add(globalID);
                break;

            case Tool.toolType.PROFILE:
                TopographicProfileTool.instanceList.Add(NewToolInstance);
                break;

            case Tool.toolType.RULER:
                RulerTool.instanceList.Add(NewToolInstance);
                RulerTool.RulerMap.Add(globalID);

                break;
            case Tool.toolType.SURFACE:
                SurfaceTool.instanceList.Add(NewToolInstance);
                SurfaceTool.surfaceMap.Add(globalID);

                break;
        }
        
        //ToolInstances.Add(NewToolInstance);
        return NewToolInstance;
    }

    public IEnumerator ShowMessage(string message, float delay)
    {

        NotificationText.GetComponent<UnityEngine.UI.Text>().text = message;
        //notificationText.GetComponent<UnityEngine.UI.Text>().enabled = true;
        NotificationPanel.SetActive(true);

        yield return new WaitForSeconds(delay);
        //notificationText.GetComponent<UnityEngine.UI.Text>().enabled = false;
        NotificationPanel.SetActive(false);

    }
}
