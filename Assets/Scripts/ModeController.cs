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

public class ModeController : MonoBehaviour {
    public GameObject WalkingModeObj, FlightModeObj, DroneModeObj;
    public GameObject MapCameraObj;

    public KeyCode WALKING_MODE_KEY = KeyCode.F8;
    public KeyCode FLIGHT_MODE_KEY = KeyCode.F9;
    public KeyCode DRONE_MODE_KEY = KeyCode.F10;

    public StateSingleton.StateMode sm;
    public float playerHalfHeight = 50;
    public float droneStartHeightOffset = 400;
   
    public void Start()
    {
        StateSingleton.stateMode = StateSingleton.StateMode.WALKING;
        PositionSingleton.playerContinousPosition = WalkingModeObj.transform.position;
        sm = StateSingleton.stateMode;
        UpdateStateMode();
    }

    void Update()
    {




        if (Input.GetKeyDown(WALKING_MODE_KEY)) StateSingleton.stateMode = StateSingleton.StateMode.WALKING;
        else if (Input.GetKeyDown(FLIGHT_MODE_KEY)) StateSingleton.stateMode = StateSingleton.StateMode.FLIGHT;
        else if (Input.GetKeyDown(DRONE_MODE_KEY)) StateSingleton.stateMode = StateSingleton.StateMode.DRONE;
        if (sm != StateSingleton.stateMode)
        {

            sm = StateSingleton.stateMode;
            //UpdateStateModeFarlocco();
            UpdateStateMode();
        }
/*
        if (OVRInput.GetDown(OVRInput.RawButton.X) && StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS && !PauseAndGUIBehaviour.isPause && !PauseAndGUIBehaviour.isModeMenu)
        {
            switch (StateSingleton.stateMode)
            {
                case StateSingleton.StateMode.WALKING:
                    StateSingleton.stateMode = StateSingleton.StateMode.FLIGHT;
                    break;
                case StateSingleton.StateMode.FLIGHT:
                    StateSingleton.stateMode = StateSingleton.StateMode.DRONE;
                    break;
                case StateSingleton.StateMode.DRONE:
                    StateSingleton.stateMode = StateSingleton.StateMode.WALKING;
                    break;
            }
        }
        */

       

    }

    void FixedUpdate()
    {

        if (StateSingleton.stateMode == StateSingleton.StateMode.WALKING)
        {

            FlightModeObj.SetActive(false);
            DroneModeObj.SetActive(false);
        }
        else if (StateSingleton.stateMode == StateSingleton.StateMode.FLIGHT)
        {
            WalkingModeObj.SetActive(false);
            DroneModeObj.SetActive(false);
        }
        else if (StateSingleton.stateMode == StateSingleton.StateMode.DRONE)
        {
             WalkingModeObj.SetActive(false);
             FlightModeObj.SetActive(false);
        }


    }

    void UpdateStateModeFarlocco()
    {

        WalkingModeObj.SetActive(false);
        FlightModeObj.SetActive(false);
        DroneModeObj.SetActive(false);

    }



    public void UpdateStateMode()
    {
        RaycastHit hit;
        switch(sm)
        {
            case StateSingleton.StateMode.WALKING:

                 if (Physics.Raycast(PositionSingleton.playerContinousPosition, Vector3.down, out hit, 100000))
                {

                    PositionSingleton.playerContinousPosition = hit.point + Vector3.up*playerHalfHeight;
                }
                WalkingModeObj.transform.position = PositionSingleton.playerContinousPosition;
                WalkingModeObj.transform.eulerAngles = PositionSingleton.playerContinousRotation;
                WalkingModeObj.SetActive(true);
               
                MapCameraObj.GetComponent<MapCameraBehaviour>().target = WalkingModeObj;

                if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP)
                {
                    GameObject CanvasL = GameObject.Find("CanvasL");
                    GameObject CanvasR = GameObject.Find("CanvasR");
                    GameObject VP = WalkingModeObj.transform.Find("3DVPCamera").gameObject;
                    CanvasL.GetComponent<Canvas>().worldCamera = VP.transform.Find("Left").GetComponent<Camera>();
                    CanvasR.GetComponent<Canvas>().worldCamera = VP.transform.Find("Right").GetComponent<Camera>();
                }

               // FlightModeObj.SetActive(false);
               // DroneModeObj.SetActive(false);

                break;
            case StateSingleton.StateMode.FLIGHT:

                FlightModeObj.transform.position =
                    new Vector3(PositionSingleton.playerContinousPosition.x,
                    PositionSingleton.playerContinousPosition.y+FlightModeObj.GetComponent<TopCameraControlller>().STARTHEIGHT,
                    PositionSingleton.playerContinousPosition.z);
                FlightModeObj.transform.eulerAngles = Vector3.up*PositionSingleton.playerContinousRotation.y;
                FlightModeObj.SetActive(true);

                MapCameraObj.GetComponent<MapCameraBehaviour>().target = FlightModeObj;

                if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP)
                {
                    GameObject CanvasL = GameObject.Find("CanvasL");
                    GameObject CanvasR = GameObject.Find("CanvasR");
                    GameObject VP = FlightModeObj.transform.Find("3DVPCamera").gameObject;
                    CanvasL.GetComponent<Canvas>().worldCamera = VP.transform.Find("Left").GetComponent<Camera>();
                    CanvasR.GetComponent<Canvas>().worldCamera = VP.transform.Find("Right").GetComponent<Camera>();
                }

//                    WalkingModeObj.SetActive(false);
  //                  DroneModeObj.SetActive(false);



                break;
            case StateSingleton.StateMode.DRONE:
     		if (Physics.Raycast(PositionSingleton.playerContinousPosition, Vector3.down, out hit, 1000))
                {
                    PositionSingleton.playerContinousPosition = hit.point + droneStartHeightOffset * Vector3.up;
                }

                DroneModeObj.transform.position = PositionSingleton.playerContinousPosition;
                DroneModeObj.transform.eulerAngles = PositionSingleton.playerContinousRotation;
                DroneModeObj.SetActive(true);

                MapCameraObj.GetComponent<MapCameraBehaviour>().target = DroneModeObj;

                if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP)
                {
                    GameObject CanvasL = GameObject.Find("CanvasL");
                    GameObject CanvasR = GameObject.Find("CanvasR");
                    GameObject VP = DroneModeObj.transform.Find("3DVPCamera").gameObject;
                    CanvasL.GetComponent<Canvas>().worldCamera = VP.transform.Find("Left").GetComponent<Camera>();
                    CanvasR.GetComponent<Canvas>().worldCamera = VP.transform.Find("Right").GetComponent<Camera>();
                }
               // WalkingModeObj.SetActive(false);
              //  FlightModeObj.SetActive(false);

                break;
        }


    }
}
