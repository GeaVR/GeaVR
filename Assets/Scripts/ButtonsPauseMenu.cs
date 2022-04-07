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
        Show(MenuGuiToShow, !MenuGuiToShow.activeSelf);

    }

    void GPS2()
    {
        PauseAndGUIBehaviour.isGPS2 = true;
        PauseAndGUIBehaviour.isPause = true;
        Show(MenuGuiToShow, !MenuGuiToShow.activeSelf);
    }

    void GPS_INFO_ON_MAP()
    {
        PauseAndGUIBehaviour.isGpsInfoOnMap = !PauseAndGUIBehaviour.isGpsInfoOnMap;
    }

    void Help()
    {
    }

    void Placemark()
    {
    }

    void Screenshot()
    {
        PauseAndGUIBehaviour.isScreenshot = true;
    }

    void TopographicProfile()
    {
        PauseAndGUIBehaviour.isPause = false;
    }

    void Ruler()
    {
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