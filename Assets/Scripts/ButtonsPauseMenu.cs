using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System;

public class ButtonsPauseMenu : MonoBehaviour
{
    //Scelti dall'editor di unity
    public enum Action
    {
        ExitToTitle, Exit,
        Menu, Settings, Help,
        Placemark, PolyLine, Polygon, TopographicProfile,
        Ruler, Compass, Notebook, Screenshot,
        GPS1, GPS2, GPS3, GPS_INFO_ON_MAP,
        WalkMode, FlightMode, DroneMode,
        PlacemarkTeleport, ScreenshotForWaypoint
    };
    public Action actionEnum;
    public GameObject MenuGuiToShow;
    UnityEvent OculusTouchTrigger, OculusTouchTriggerOn;
    private UnityAction action;
    public GameObject[] GuiToShowPause;
    public GameObject GuiToHidePauseScreenshot;

    bool screenshot = false;


    void Start()
    {



        switch (actionEnum)
        {
            case Action.Exit:
                action = Exit;
                break;

            case Action.ExitToTitle:
                action = ExitToTitle;
                break;

            case Action.Menu:
                action = Menu;
                break;
            case Action.Settings:
                action = Settings;
                break;

            case Action.Placemark:
                action = Placemark;
                break;

            case Action.Screenshot:
                action = Screenshot;
                break;

            case Action.TopographicProfile:
                action = TopographicProfile;
                break;

            case Action.Ruler:
                action = Ruler;
                break;

            case Action.GPS1:
                action = GPS1;
                break;

            case Action.GPS2:
                action = GPS2;
                break;

            case Action.GPS_INFO_ON_MAP:
                action = GPS_INFO_ON_MAP;
                break;

            case Action.Help:
                action = Help;
                break;

            case Action.WalkMode:
                action = WalkMode;
                break;

            case Action.FlightMode:
                action = FlightMode;
                break;

            case Action.DroneMode:
                action = DroneMode;
                break;

            case Action.PlacemarkTeleport:
                action = PlacemarkTeleport;
                break;

            case Action.ScreenshotForWaypoint:
                action = ScreenshotForWaypoint;
                break;

        }
        GetComponent<Button>().onClick.AddListener(action);
        OculusTouchTrigger = new UnityEvent();
        OculusTouchTrigger.AddListener(action);
        OculusTouchTriggerOn = new UnityEvent();
        OculusTouchTriggerOn.AddListener(() => { print("Oculus Touch Trigger On"); });

    }

    void ExitToTitle()
    {
        SceneManager.LoadScene("Splash_screen");
        Time.timeScale = 1;
    }

    void Exit()
    {
        Application.Quit();
    }

    void Menu()
    {
        Show(MenuGuiToShow, !MenuGuiToShow.activeSelf);
    }

    void Settings()
    {
        //ShowSons(true);
        Show(MenuGuiToShow, !MenuGuiToShow.activeSelf);

    }

    void GPS2()
    {

        //ShowSons(true);

        PauseAndGUIBehaviour.isGPS2 = true;
        PauseAndGUIBehaviour.isPause = true;

        Show(MenuGuiToShow, !MenuGuiToShow.activeSelf);

    }

    void GPS_INFO_ON_MAP()
    {

        PauseAndGUIBehaviour.isGpsInfoOnMap = !PauseAndGUIBehaviour.isGpsInfoOnMap;

        // PauseAndGUIBehaviour.isPause = true;

    }

    void Help()
    {

    }

    void Placemark()
    {
        //VirtualMeter.isActiveWaypoint = true;
    }


    void Screenshot()
    {

        //PauseAndGUIBehaviour.isPauseforScreenshot = true;

        PauseAndGUIBehaviour.isScreenshot = true;
     //   CameraTools.isActiveScreenshot = true;


    }



    void TopographicProfile()
    {
        //VirtualMeter.isActiveTopographicProfile = true;
        PauseAndGUIBehaviour.isPause = false;
    }

    void Ruler()
    {
        //VirtualMeter.isActiveDistance = true;
        PauseAndGUIBehaviour.isPause = false;
    }

    void GPS1()
    {

        PauseAndGUIBehaviour.isCoordinate = !PauseAndGUIBehaviour.isCoordinate;
        PauseAndGUIBehaviour.isPause = false;
    }


    void WalkMode()
    {
        StateSingleton.stateMode = StateSingleton.StateMode.WALKING;

        if (PauseAndGUIBehaviour.isModeMenu)
            PauseAndGUIBehaviour.isModeMenu = false;

        PauseAndGUIBehaviour.isPause = false;
    }

    void FlightMode()
    {
        if (PauseAndGUIBehaviour.isModeMenu)
            PauseAndGUIBehaviour.isModeMenu = false;

        StateSingleton.stateMode = StateSingleton.StateMode.FLIGHT;
        PauseAndGUIBehaviour.isPause = false;
    }

    void DroneMode()
    {
        if (PauseAndGUIBehaviour.isModeMenu)
            PauseAndGUIBehaviour.isModeMenu = false;
            PauseAndGUIBehaviour.isModeMenu = false;

        StateSingleton.stateMode = StateSingleton.StateMode.DRONE;
        PauseAndGUIBehaviour.isPause = false;
    }

    void PlacemarkTeleport()
    {
        Vector3 position = GetComponent<PlacemarkData>().position;
        switch (StateSingleton.stateMode)
        {
            case StateSingleton.StateMode.WALKING:
                break;
            case StateSingleton.StateMode.FLIGHT:
                break;
            case StateSingleton.StateMode.DRONE:
                break;
        }
    }

    void ScreenshotForWaypoint()
    {
        //PauseAndGUIBehaviour.isScreenshot = true;

       // CameraTools.isActiveScreenshotForWaypoint = true;
    }

    //General Purpose Functions

    void ShowSons(bool value)
    {
        foreach (Transform tr in GetComponentsInChildren<Transform>(true))
        {
            if (tr.gameObject != this.gameObject && tr.GetSiblingIndex() != 1)
                tr.gameObject.SetActive(value);
        }
    }


    void Show(GameObject gb, bool value)
    {
        gb.SetActive(value);
    }

    //OCULUS TRIGGER FUNCTION
    public void OculusTriggerButton()
    {
        OculusTouchTrigger.Invoke();
    }

    public void OculusTriggerButtonOn()
    {
        OculusTouchTriggerOn.Invoke();
    }
}
