using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using TMPro;
using SharpKml;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System.Globalization;
using UnityEngine.EventSystems;

/// <summary>
/// 
///  ToolInstance is a component that handles a created Tool:
///     polylines, polygons, placemarks, profile...
///     
/// also handles the created menus for the tool
/// 
/// </summary>
/// 
public class ToolInstance : MonoBehaviour {
    
    // ID handling
    public static int StaticID = 1;
    public string ID;
    private int Index;
    public Tool.toolType instanceToolType;

    //private List<decimal> ValueList; 
    public List<GameObject> PlacemarkList;

    public List<GameObject> PolylineList;
    public List<GameObject> PolygonList;
    public List<GameObject> RulerList;
    public List<GameObject> ProfileList;
    public List<GameObject> SurfaceList;
    public Dictionary<string, decimal> ValueDict;

    public string Notes;
    // Menu Handling

    public GameObject WorldMenu, OculusMenu, GUIMenu, NotesMenu;
    public string ToolTitle, ToolText, CustomTxt;
    public DateTime creationDate;
    private ToolController toolControllerComponent;

    // only 1 tool should be visible at a time
    // this is used to track which
    private static ToolInstance CurrentlyVisible = null;

    // flags to affect behaviour

    public bool ShouldDestroyOnClose = false;
    public bool ShouldShowWorldMenu = true;
    private bool isSaving = false;
    private bool isClosing = false;

    // Use this for initialization
    void Start()
    {

    }

    public void PostStart(ToolController _toolController, string _Title, string _Text, string _Desc, Tool.toolType _ToolType,
        Dictionary<string, decimal> Values, List<GameObject> Placemarks, DateTime _creationDate,
        bool DestroyOnClose = false, bool hasGraph = false, bool ShowWorldMenu = true, int localID=0, string globalID = "", bool showToolSummaryUI = true)
    {
        CurrentlyVisible = this;
        ID = globalID;
        Index = localID;
        // ID = StaticID;
        // StaticID = StaticID + 1;
        instanceToolType = _ToolType;
        creationDate = _creationDate;

        toolControllerComponent = _toolController;

        ToolTitle = _Title + " " + Index.ToString("000000");
        ToolText = _Text;
        CustomTxt = _Desc;

        ValueDict = Values;

        // PlacemarkList = Placemarks;



        switch (_ToolType)
        {
            case Tool.toolType.PLACEMARK:
                PlacemarkList = Placemarks;
                break;

            case Tool.toolType.POLYGON:
                PolygonList = Placemarks;
                break;

            case Tool.toolType.LINE:
                PolylineList = Placemarks;

                break;

            case Tool.toolType.PROFILE:
                ProfileList = Placemarks;
                break;

            case Tool.toolType.RULER:
                RulerList = Placemarks;
                break;
            case Tool.toolType.SURFACE:
                SurfaceList = Placemarks;
                break;
        }

        ShouldDestroyOnClose = DestroyOnClose;
        ShouldShowWorldMenu = ShowWorldMenu;

        // if Oculus
        if ( showToolSummaryUI && (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)) 
        {
            BoxCollider tempcoll;
            if (toolControllerComponent.ViewControllerGameObject.GetComponent<ViewController>().ExperimentalOculusUI )
            {
                // Create in-World menu
                WorldMenu = Instantiate((!hasGraph) ? toolControllerComponent.ToolMenuPrefab : toolControllerComponent.ToolGraphPrefab);
                WorldMenu.name = "WorldMenu" + ID.ToString();
                WorldMenu.transform.position = Placemarks[0].transform.position + (Vector3.up * toolControllerComponent.WorldUIScale * toolControllerComponent.MarkerScale * 250);
                WorldMenu.transform.localScale *= toolControllerComponent.WorldUIScale * toolControllerComponent.MarkerScale;

                // set world menu to update info when maximised
                Transform WorldMenuCanvas = WorldMenu.transform.Find("Canvas");
                //WorldMenuCanvas.Find("Maximise").gameObject.GetComponent<Button>().onClick.AddListener(ShowMenus);
                if (WorldMenuCanvas.Find("Cancel").gameObject.GetComponent<Button>().interactable)
                    WorldMenuCanvas.Find("Cancel").gameObject.GetComponent<Button>().onClick.AddListener(DestroyInstance);
                if (WorldMenuCanvas.Find("Close").gameObject.GetComponent<Button>().interactable)
                    WorldMenuCanvas.Find("Close").gameObject.GetComponent<Button>().onClick.AddListener(DestroyOrClose);
                if (WorldMenuCanvas.Find("Notes").gameObject.GetComponent<Button>().interactable)
                    WorldMenuCanvas.Find("Notes").gameObject.GetComponent<Button>().onClick.AddListener(ShowNotes);
                if (WorldMenuCanvas.Find("Export").gameObject.GetComponent<Button>().interactable)
                    WorldMenuCanvas.Find("Export").gameObject.GetComponent<Button>().onClick.AddListener(SaveOutput);
                if (WorldMenuCanvas.Find("Confirm").gameObject.GetComponent<Button>().interactable)
                    WorldMenuCanvas.Find("Confirm").gameObject.GetComponent<Button>().onClick.AddListener(ConfirmMeasure);

                // Add 3d colliders to world menus                 
                foreach (Transform tr in WorldMenu.GetComponentsInChildren<Transform>(true))
                {
                    Button _button = tr.gameObject.GetComponent<Button>();
                    if (_button != null)
                    {
                        tempcoll = tr.gameObject.AddComponent<BoxCollider>() as BoxCollider;
                        tempcoll.isTrigger = true;
                        tempcoll.size = new Vector3(
                            tr.gameObject.GetComponent<RectTransform>().sizeDelta.x,
                            tr.gameObject.GetComponent<RectTransform>().sizeDelta.y,
                            0.01f
                        );
                    }
                }
            }

            // Oculus Menu
            OculusMenu = Instantiate((!hasGraph) ? toolControllerComponent.ToolMenuPrefab : toolControllerComponent.ToolGraphPrefab);

            // Add 3d colliders to OculusMenu            
            foreach (Transform tr in OculusMenu.GetComponentsInChildren<Transform>(true))
            {
                Button _button = tr.gameObject.GetComponent<Button>();
                if (_button != null)
                {
                    tempcoll = tr.gameObject.AddComponent<BoxCollider>() as BoxCollider;
                    tempcoll.isTrigger = true;
                    tempcoll.size = new Vector3(
                        tr.gameObject.GetComponent<RectTransform>().sizeDelta.x,
                        tr.gameObject.GetComponent<RectTransform>().sizeDelta.y,
                        0.01f
                    );
                }
            }

            OculusMenu.name = "OculusMenu" + ID.ToString();

            OculusMenu.transform.SetParent(toolControllerComponent.OculusCanvas.transform, false);
            OculusMenu.transform.localScale = Vector3.one;
            OculusMenu.transform.localPosition = Vector3.zero;

            //


            Transform OculusMenuCanvas = OculusMenu.transform.Find("Canvas");

            OculusMenuCanvas.Find("Notes").gameObject.GetComponent<Button>().interactable = true;
            //OculusMenuCanvas.Find("Maximise").gameObject.GetComponent<Button>().onClick.AddListener(ShowMenus);
            if (OculusMenuCanvas.Find("Cancel").gameObject.GetComponent<Button>().interactable)
                OculusMenuCanvas.Find("Cancel").gameObject.GetComponent<Button>().onClick.AddListener(DestroyInstance);
            if (OculusMenuCanvas.Find("Close").gameObject.GetComponent<Button>().interactable)
                OculusMenuCanvas.Find("Close").gameObject.GetComponent<Button>().onClick.AddListener(DestroyOrClose);
            

            if (OculusMenuCanvas.Find("Notes").gameObject.GetComponent<Button>().interactable)
                OculusMenuCanvas.Find("Notes").gameObject.GetComponent<Button>().onClick.AddListener(enableNotes);

            if (OculusMenuCanvas.Find("Export").gameObject.GetComponent<Button>().interactable)
                OculusMenuCanvas.Find("Export").gameObject.GetComponent<Button>().onClick.AddListener(SaveOutput);
            if (OculusMenuCanvas.Find("Confirm").gameObject.GetComponent<Button>().interactable)
                OculusMenuCanvas.Find("Confirm").gameObject.GetComponent<Button>().onClick.AddListener(ConfirmMeasure);


            if (hasGraph)
            {
                if (OculusMenuCanvas.Find("Export").gameObject.GetComponent<Button>().interactable)
                    OculusMenuCanvas.Find("Export").gameObject.GetComponent<Button>().onClick.AddListener(SaveTopographicGraphToCSV);
            }

        }


        if (showToolSummaryUI)
        {
            // 2D Menu   
            GUIMenu = Instantiate((!hasGraph) ? toolControllerComponent.ToolMenuPrefab : toolControllerComponent.ToolGraphPrefab);
            GUIMenu.name = "GUIMenu" + Index.ToString();
            GUIMenu.GetComponentInChildren<Canvas>(true).renderMode = RenderMode.ScreenSpaceOverlay;

            // add listener to close menus on cancel
            /* 
            //old version
                   GUIMenu.transform.Find("Canvas").transform.Find("Cancel").gameObject.GetComponent<Button>().onClick.AddListener(DestroyInstance);
                   GUIMenu.transform.Find("Canvas").transform.Find("Close").gameObject.GetComponent<Button>().onClick.AddListener(DestroyOrClose);

                   // update menus
                   ShowMenus();
                   toolControllerComponent.StartCoroutine(MenuCoroutine());
            */

            Transform GUIMenuCanvas = GUIMenu.transform.Find("Canvas").transform;
            GUIMenuCanvas.Find("Cancel").gameObject.GetComponent<Button>().onClick.AddListener(DestroyInstance);
            GUIMenuCanvas.Find("Close").gameObject.GetComponent<Button>().onClick.AddListener(DestroyOrClose);
            GUIMenuCanvas.Find("Notes").gameObject.GetComponent<Button>().onClick.AddListener(ShowNotes);
            GUIMenuCanvas.Find("Export").gameObject.GetComponent<Button>().onClick.AddListener(SaveOutput);
            GUIMenuCanvas.Find("Confirm").gameObject.GetComponent<Button>().onClick.AddListener(ConfirmMeasure);

            // update menus
            //ShowMenus();

            CurrentlyVisible = this;
            UpdateMenus();

            toolControllerComponent.StartCoroutine(MenuCoroutine());
        }
    }

    public string GetID()
    {
        return ID;
    }

    // menu handling

    public void UpdateGraphs(LineRenderer lr)
    {
        //Plotting graph - GUI
        UIGraphRenderer graph = GUIMenu.GetComponentInChildren<UIGraphRenderer>(true);
        graph.ClearPoints();

        if (lr.positionCount <= 0)
        {
            Debug.Log("error: no position information");
            return;
        }

        Vector3Decimal realPosition = new Vector3Decimal();
        Vector3 noZ = new Vector3(1.0f, 0.0f, 1.0f);
        Vector3 firstPosFlat = Vector3.Scale(lr.GetPosition(0), noZ);

        for (int i = 0; i < lr.positionCount; i++)
        {
            realPosition = VirtualMeter.CalculateGPSPosition(lr.GetPosition(i));
            graph.AddPoint(new Vector2(Vector3.Distance(firstPosFlat, Vector3.Scale(lr.GetPosition(i), noZ)), (float)realPosition.y));
        }

        //Plotting graph - oculus
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
        {
            // plotting in world UI
            if (toolControllerComponent.ViewControllerGameObject.GetComponent<ViewController>().ExperimentalOculusUI)
            {
                graph = WorldMenu.GetComponentInChildren<UIGraphRenderer>(true);
                graph.ClearPoints();

                for (int i = 0; i < lr.positionCount; i++)
                {
                    realPosition = VirtualMeter.CalculateGPSPosition(lr.GetPosition(i));
                    graph.AddPoint(new Vector2(Vector3.Distance(firstPosFlat, Vector3.Scale(lr.GetPosition(i), noZ)), (float)realPosition.y));
                }
            }

            // plotting in oculus UI
            graph = OculusMenu.GetComponentInChildren<UIGraphRenderer>(true);
            graph.ClearPoints();

            for (int i = 0; i < lr.positionCount; i++)
            {
                realPosition = VirtualMeter.CalculateGPSPosition(lr.GetPosition(i));
                graph.AddPoint(new Vector2(Vector3.Distance(firstPosFlat, Vector3.Scale(lr.GetPosition(i), noZ)), (float)realPosition.y));
            }
        }
    }

    public void OnUpdateString(string _newNotes)
    {
        Notes = _newNotes;
    }


    public void enableNotes()
    {
        GameObject.Find("Canvas").gameObject.transform.Find("ToolMenu").gameObject.transform.Find("Group2").gameObject.transform.Find("Field_notebook").gameObject.GetComponent<FieldNotes>().setMenuToHide(GUIMenu);
        GameObject.Find("Canvas").gameObject.transform.Find("ToolMenu").gameObject.transform.Find("Group2").gameObject.transform.Find("Field_notebook").gameObject.GetComponent<FieldNotes>().setOutputString(CustomTxt);
        GameObject.Find("Canvas").gameObject.transform.Find("ToolMenu").gameObject.transform.Find("Group2").gameObject.transform.Find("Field_notebook").gameObject.GetComponent<FieldNotes>().OnUse();

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("ToolMenu").gameObject.transform.Find("Group2").gameObject.transform.Find("Field_notebook").gameObject.GetComponent<FieldNotes>().setMenuToHide(OculusMenu);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("ToolMenu").gameObject.transform.Find("Group2").gameObject.transform.Find("Field_notebook").gameObject.GetComponent<FieldNotes>().setOutputString(CustomTxt);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("ToolMenu").gameObject.transform.Find("Group2").gameObject.transform.Find("Field_notebook").gameObject.GetComponent<FieldNotes>().OnUse();
        }
    }


    public void ShowNotes()
    {
   
        NotesMenu = Instantiate(toolControllerComponent.NotesMenuPrefab);
        NotesMenu.name = "GUIMenu" + ID.ToString();
        NotesMenu.GetComponentInChildren<Canvas>(true).renderMode = RenderMode.ScreenSpaceOverlay;

        // add listener to close menus on cancel
        NotesMenu.transform.Find("Canvas").transform.Find("Close").gameObject.GetComponent<Button>().onClick.AddListener(HideNotes);
        NotesMenu.transform.Find("Canvas").transform.Find("Export").gameObject.GetComponent<Button>().onClick.AddListener(SaveNotesToTXT);
        NotesMenu.GetComponentsInChildren<TMP_InputField>(true)[0].onEndEdit.AddListener(OnUpdateString);

    }

    public void HideNotes()
    {        
        if (NotesMenu) { Destroy(NotesMenu); }
    }

    //saving / loading

    public void SaveOutput()
    {

        StartCoroutine(ShowNotificationInLabel("Done!", 1.5f));

        switch (instanceToolType)
        {
            case Tool.toolType.PLACEMARK:
                PlacemarkTool.SaveSingleInstance(this);
                break;

            case Tool.toolType.POLYGON:
                PolygonTool.SaveSingleInstance(this);
                break;

            case Tool.toolType.LINE:
                PolylineTool.SaveSingleInstance(this);
                break;

            case Tool.toolType.PROFILE:
                TopographicProfileTool.SaveSingleInstance(this);
                break;

            case Tool.toolType.RULER:
                RulerTool.SaveSingleInstance(this);
                break;
            case Tool.toolType.SURFACE:
                SurfaceTool.SaveSingleInstance(this);
                break;
        }
    }
        
    // other

    public void SaveTopographicGraphToCSV()
    {
        LineRenderer lr = PlacemarkList[0].GetComponent<LineRenderer>();

        var directory = new DirectoryInfo(Application.dataPath);
        var directoryPath = Path.Combine(directory.Parent.FullName, "Outputs/Graphs");
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

        var path = Path.Combine(directoryPath, string.Format("{0}_graph.csv", ToolTitle.ToString()));

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        var sr = File.CreateText(path);
        string csvData = "";
        sr.WriteLine("Point, Lat(°), Lon(°) z(m)");
        Vector3Decimal realPosition = new Vector3Decimal();

        for (int i = 0; i < lr.positionCount; ++i)
        {
            csvData = "";
            csvData += i.ToString() + ",";

            //realPosition = VirtualMeter.CalculateRealPositionOfPoint(lr.GetPosition(i));
            // The computation schould be done with real position
            realPosition = VirtualMeter.CalculateGPSPosition(lr.GetPosition(i));
            csvData += realPosition.z.ToString("0.0000000000000") + ",";
            csvData += realPosition.x.ToString("0.0000000000000") + ",";
            csvData += realPosition.y.ToString("0.0000000000000");
            sr.WriteLine(csvData);


        }

        sr.Close();

        //StartCoroutine(ShowNotification("Profile has been exported", 1));

    }

    public void SaveNotesToTXT()
    {

        var directory = new DirectoryInfo(Application.dataPath);
        var directoryPath = Path.Combine(directory.Parent.FullName, "Outputs");

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

        var path = Path.Combine(directoryPath, string.Format("Notes_{0}.txt", ToolTitle.ToString()));

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        var sr = File.CreateText(path);
        sr.WriteLine(Notes);
        sr.Close();
    }


    // private

    private void DestroyMenus()
    {
        if (WorldMenu != null) { Destroy(WorldMenu); }
        if (OculusMenu != null) { Destroy(OculusMenu); }
        if (GUIMenu != null) { Destroy(GUIMenu); }
        if (NotesMenu != null) { Destroy(NotesMenu); }
    }

    private void DestroyOrClose()
    {
        isClosing = true;

        DestroyInstance();
        ToolController.ToolControllerInterfaceIsCurrentlyRunning = false;

        toolControllerComponent.OculusMasterObject.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.2783019f, 0.3061422f, 1, 1);


    }
/*
    public void DestroyAllInstance()
    {

        DestroyInstance();
    }
    */
    public void DestroySingleInstance()
    {

        if (CurrentlyVisible == this)
        {
            CurrentlyVisible = null;
        }

        switch (instanceToolType)
        {
            case Tool.toolType.PLACEMARK:
               
                foreach (var p in PlacemarkList)
                {
                    Destroy(p);
                }
                break;

            case Tool.toolType.POLYGON:
                foreach (var p in PolygonList)
                {
                    Destroy(p);
                }
                break;

            case Tool.toolType.LINE:
               
                foreach (var p in PolylineList)
                {
                    Destroy(p);
                }
                break;

            case Tool.toolType.PROFILE:
                // TopographicProfileTool.instanceList.Add(NewToolInstance);
                foreach (var p in ProfileList)
                {
                    Destroy(p);
                }

                break;

            case Tool.toolType.RULER:
                foreach (var p in RulerList)
                {
                    Destroy(p);
                }
                break;
            case Tool.toolType.SURFACE:
                foreach (var p in SurfaceList)
                {
                    Destroy(p);
                }
                break;
        }


       

      //   StaticID--;


        if (gameObject != null)
        {
            Destroy(gameObject);
        } 

    }
  


    public void DestroyInstance()
    {

        Debug.Log("Called:DestroyInstance ");
        //StopAllCoroutines();
        DestroyMenus();

        if (CurrentlyVisible == this)
        {
            CurrentlyVisible = null;
        }
        


  
            switch (instanceToolType)
            {
                case Tool.toolType.PLACEMARK:
                    
                    foreach (var p in PlacemarkList)
                    {
                        Destroy(p);
                    }

                   


                    if (gameObject != null)
                    {
                        Destroy(gameObject);
                    }

                    PlacemarkTool.instanceList.Remove(gameObject);
                    PlacemarkTool.localID--;
               
                    break;

                case Tool.toolType.POLYGON:
                    foreach (var p in PolygonList)
                    {
                        Destroy(p);
                    }

                    


                    if (gameObject != null)
                    {
                        Destroy(gameObject);
                    }

                    PolygonTool.instanceList.Remove(gameObject);
                    PolygonTool.localID--;
                break;

                case Tool.toolType.LINE:
                    
                    foreach (var p in PolylineList)
                    {
                        Destroy(p);
                    }

                   


                    if (gameObject != null)
                    {
                        Destroy(gameObject);
                    }

                    PolylineTool.instanceList.Remove(gameObject);
                    PolylineTool.localID--;
                break;

                case Tool.toolType.PROFILE:
                // TopographicProfileTool.instanceList.Add(NewToolInstance);

                    foreach (var p in ProfileList)
                    {
                        Destroy(p);
                    }

                    if (gameObject != null)
                    {
                        Destroy(gameObject);
                    }

                    TopographicProfileTool.instanceList.Remove(gameObject);

            
                break;

                case Tool.toolType.RULER:
                    foreach (var p in RulerList)
                    {
                        Destroy(p);
                    }

               


                    if (gameObject != null)
                    {
                        Destroy(gameObject);
                    }

                    RulerTool.instanceList.Remove(gameObject);
                    RulerTool.localID--;
                break;

                case Tool.toolType.SURFACE:
                    foreach (var p in SurfaceList)
                    {
                        Destroy(p);
                    }

               


                    if (gameObject != null)
                    {
                        Destroy(gameObject);
                    }

                    SurfaceTool.instanceList.Remove(gameObject);
                    SurfaceTool.localID--;
                break;
            }

   


        if (!isClosing)
        {
            toolControllerComponent.MeasurementControlUI.SetActive(true);
            if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            {
                GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.SetActive(true);
                GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").transform.localPosition = new Vector3(0.0f, -530.0f, 0.0f);

            }
        }

        PauseAndGUIBehaviour.isToolMenu = false;
        ToolController.ToolIsCurrentlyRunning = false;
        ToolController.ToolControllerInterfaceIsCurrentlyRunning = true;



    }

    public void ConfirmMeasure()
    {
       
       // tmpNote = null;
         DestroyMenus();
 /*
        switch (instanceToolType)
        {
            case Tool.toolType.PLACEMARK:
               
                break;

            case Tool.toolType.POLYGON:
                
                break;

            case Tool.toolType.LINE:
                
                break;

            case Tool.toolType.PROFILE:
               
                break;

            case Tool.toolType.RULER:

                foreach (var p in PlacemarkList)
                {
                    Destroy(p);
                }

                if (gameObject != null)
                {
                    Destroy(gameObject);
                }
                break;
        }
*/
        ToolController.ToolIsCurrentlyRunning = false;
        ToolController.ToolControllerInterfaceIsCurrentlyRunning = true;

        toolControllerComponent.MeasurementControlUI.SetActive(true);
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").transform.localPosition = new Vector3(0.0f, -530.0f, 0.0f);

        }
        if (ShouldDestroyOnClose)
        {
            //DestroyInstance();
            switch (instanceToolType)
            {
                case Tool.toolType.PLACEMARK:
                    foreach (var placemark in PlacemarkList)
                    {
                        foreach (var SpriteRen in placemark.transform.GetComponentsInChildren<SpriteRenderer>(true))
                        {
                            SpriteRen.enabled = false;
                        }
                    }

                    break;

                case Tool.toolType.POLYGON:

                    break;

                case Tool.toolType.LINE:
                    foreach (var line in PolylineList)
                    {
                        foreach (var SpriteRen in line.transform.GetComponentsInChildren<SpriteRenderer>(true))
                        {
                            SpriteRen.enabled = false;
                        }
                    }

                    break;

                case Tool.toolType.PROFILE:

                    break;

                case Tool.toolType.RULER:

                    break;
            }



            
        }
        PauseAndGUIBehaviour.isToolMenu = false;
    }

    private void UpdateMenus()
    {
        // Update Text
        if (WorldMenu)
        {
            WorldMenu.GetComponentsInChildren<Text>(true)[0].text = ToolTitle;
            WorldMenu.GetComponentsInChildren<InputField>(true)[0].text = ToolText;
        }

        if (OculusMenu)
        {
            OculusMenu.GetComponentsInChildren<Text>(true)[0].text = ToolTitle;
            OculusMenu.GetComponentsInChildren<InputField>(true)[0].text = ToolText;
        }

        if (GUIMenu)
        {
            GUIMenu.GetComponentsInChildren<Text>(true)[0].text = ToolTitle;
            GUIMenu.GetComponentsInChildren<InputField>(true)[0].text = ToolText;
        }        
    }

    // coroutine

    public IEnumerator MenuCoroutine()
    {
        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();


     
        bool menuShouldStayOpen = true;
        while (menuShouldStayOpen && !isClosing)
        {
            Cursor.visible = true;
            //menuShouldStayOpen = !(Tool.checkIfToolShouldQuit());
            yield return wfeof;
        }

        
        Cursor.visible = false;
        ToolController.ToolIsCurrentlyRunning = false;

        if (ShouldDestroyOnClose)
        {
            DestroyInstance();
        }
        else
        {
            DestroyMenus();
        }
    }


    IEnumerator ShowNotificationInLabel(string message, float delay)
    {
       
        if (GameObject.Find(GUIMenu.name)!=null)
            GameObject.Find(GUIMenu.name).gameObject.transform.Find("Canvas").gameObject.transform.Find("LowerPanel").transform.Find("downlabelastooltip").GetComponent<Text>().text = message;
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
            GameObject.Find(OculusMenu.name).gameObject.transform.Find("Canvas").gameObject.transform.Find("LowerPanel").transform.Find("downlabelastooltip").GetComponent<Text>().text = message;

        if (GameObject.Find(GUIMenu.name)!=null)
            GameObject.Find(GUIMenu.name).gameObject.transform.Find("Canvas").gameObject.transform.Find("LowerPanel").transform.Find("downlabelastooltip").GetComponent<Text>().color = new Color(1, 0, 0, 1); ;
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
            GameObject.Find(OculusMenu.name).gameObject.transform.Find("Canvas").gameObject.transform.Find("LowerPanel").transform.Find("downlabelastooltip").GetComponent<Text>().color = new Color(1, 0, 0, 1); ;

        
        /*
        GameObject.Find("Canvas").gameObject.transform.Find("GpsTrackControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = message;
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsTrackControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = message;
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsTrackControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().color = new Color(1, 0, 0, 1); ;
        GameObject.Find("Canvas").gameObject.transform.Find("GpsTrackControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().color = new Color(1, 0, 0, 1); ;
        */

        yield return new WaitForSeconds(delay);

        if (GameObject.Find(GUIMenu.name)!=null)
            GameObject.Find(GUIMenu.name).gameObject.transform.Find("Canvas").gameObject.transform.Find("LowerPanel").transform.Find("downlabelastooltip").GetComponent<Text>().text = "";
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
            GameObject.Find(OculusMenu.name).gameObject.transform.Find("Canvas").gameObject.transform.Find("LowerPanel").transform.Find("downlabelastooltip").GetComponent<Text>().text = "";

        if (GameObject.Find(GUIMenu.name)!=null)
            GameObject.Find(GUIMenu.name).gameObject.transform.Find("Canvas").gameObject.transform.Find("LowerPanel").transform.Find("downlabelastooltip").GetComponent<Text>().color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 1);
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
            GameObject.Find(OculusMenu.name).gameObject.transform.Find("Canvas").gameObject.transform.Find("LowerPanel").transform.Find("downlabelastooltip").GetComponent<Text>().color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 1);

        /*
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsTrackControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = "";
        GameObject.Find("Canvas").gameObject.transform.Find("GpsTrackControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = "";
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsTrackControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 1); ;
        GameObject.Find("Canvas").gameObject.transform.Find("GpsTrackControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 1); ;
  */

    }

}


