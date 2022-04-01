using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace UnityStandardAssets.Characters.FirstPerson
{

    public class ViewController : MonoBehaviour {
        public GameObject ModeController;
        public GameObject WalkingModeObj, FlightModeObj, DroneModeObj;
        public GameObject Mode3DVPWalking, Mode3DVPFlight, Mode3DVPDrone;
        public GameObject Mode2DWalking, Mode2DFlight, Mode2DDrone;
        public GameObject ModeOculusWalking, ModeOculusFlight, ModeOculusDrone;
        public GameObject OculusTouchLeft, OculusTouchRight;
        public GameObject Canvas, CanvasL, CanvasR, CanvasOculus;


        public KeyCode MODE2D_KEY = KeyCode.LeftControl;
        public KeyCode MODE3DVP_KEY = KeyCode.RightControl;
        public KeyCode MODEOCULUS_KEY = KeyCode.RightShift;

        public StateSingleton.StateView sv;
        public StateSingleton.StateView START_STATE_VIEW;

        public float oculusCanvasDistance = 1000.0f;
        public float oculusCanvasScale = 1.0f;
        public bool ExperimentalOculusUI = false;

        // Use this for initialization
        void Start()
        {
            if (StateSingleton.stateView == StateSingleton.StateView.UNSET)
                StateSingleton.stateView = START_STATE_VIEW;

            //            sv = StateSingleton.stateView;
           
           /* sv = StateSingleton.StateView.UNSET;
            ModeController.SetActive(true);
*/
//            UpdateStateView();

//TEST REVERT
    sv = StateSingleton.stateView;
            UpdateStateView();
            ModeController.SetActive(true);


        }

        // Update is called once per frame
        void Update () {

          
            if (Input.GetKeyDown(MODE2D_KEY)) StateSingleton.stateView = StateSingleton.StateView.MODE2D;
            else if (Input.GetKeyDown(MODE3DVP_KEY)) StateSingleton.stateView = StateSingleton.StateView.MODE2D_PLUS_3DVP;
            else if (Input.GetKeyDown(MODEOCULUS_KEY)) StateSingleton.stateView = StateSingleton.StateView.MODE2D_PLUS_OCULUS;
            if (sv != StateSingleton.stateView)
            {
                sv = StateSingleton.stateView;
                UpdateStateView();
            }


            if (CanvasOculus != null)
            {
                GameObject target = gameObject;
                switch (StateSingleton.stateMode) {
                    case StateSingleton.StateMode.WALKING:
                        target = ModeOculusWalking;
                        break;
                    case StateSingleton.StateMode.FLIGHT:
                        target = ModeOculusFlight;
                        break;
                    case StateSingleton.StateMode.DRONE:
                        target = ModeOculusDrone;
                        break;
                }

                if (ExperimentalOculusUI && OculusTouchLeft)
                {
                    CanvasOculus.transform.position = OculusTouchLeft.transform.position + OculusTouchLeft.transform.TransformDirection((Vector3.forward + Vector3.up * 0.5f) * 0.4f);
                    CanvasOculus.transform.localScale = new Vector3(
                        0.0008f,
                        0.0008f,
                        0.0008f
                        );
                    CanvasOculus.transform.eulerAngles = OculusTouchLeft.transform.eulerAngles;
                }
                else
                {
                    CanvasOculus.transform.position = target.transform.position + target.transform.TransformDirection(Vector3.forward * oculusCanvasDistance);
                    CanvasOculus.transform.localScale = new Vector3(
                        oculusCanvasDistance * oculusCanvasScale * 0.001f,
                        oculusCanvasDistance * oculusCanvasScale * 0.001f,
                        oculusCanvasDistance * oculusCanvasScale * 0.001f);
                    CanvasOculus.transform.eulerAngles = target.transform.eulerAngles;
                }
            }

            // We should probably move these shortcuts somewhere more appropriate
            // ToolController would be a good candidate     Martin K 06-12/18            
            //TEMP SHORTKEY
            
            if ((Input.GetKeyDown(KeyCode.Escape) || OVRInput.GetDown(OVRInput.Button.Four)) && !PauseAndGUIBehaviour.isModeMenu && !PauseAndGUIBehaviour.isGPS2 && !ToolController.ToolIsCurrentlyRunning)
            {
                PauseAndGUIBehaviour.isPause = !PauseAndGUIBehaviour.isPause;
            }
            

            //            if ((Input.GetKeyDown(KeyCode.F2) || OVRInput.GetDown(OVRInput.Button.Two)) && !PauseAndGUIBehaviour.isPause && !ToolController.ToolIsCurrentlyRunning)
            if ((Input.GetKeyDown(KeyCode.F2) || OVRInput.GetDown(OVRInput.Button.Two)) && !PauseAndGUIBehaviour.isPause && !PauseAndGUIBehaviour.isSettingsMenu && !ToolController.ToolIsCurrentlyRunning && !PauseAndGUIBehaviour.isToolMenu && !ToolController.ToolControllerInterfaceIsCurrentlyRunning)
            {
                PauseAndGUIBehaviour.isModeMenu = !PauseAndGUIBehaviour.isModeMenu;

            }
            

            if ( Input.GetKeyDown(KeyCode.F3) || OVRInput.GetDown(OVRInput.Button.One) && !PauseAndGUIBehaviour.isPause &&  !PauseAndGUIBehaviour.isModeMenu && !PauseAndGUIBehaviour.isSettingsMenu)
            {
                //             PauseAndGUIBehaviour.isPause = !PauseAndGUIBehaviour.isPause;
                if (ToolController.ToolIsCurrentlyRunning || ToolController.ToolControllerInterfaceIsCurrentlyRunning)
                {
                   // StartCoroutine(ShowNotification("Close Current Tool", 1));
                }
                else
                {
                    PauseAndGUIBehaviour.isToolMenu = !PauseAndGUIBehaviour.isToolMenu;
                }
            }

            if ( Input.GetKeyDown(KeyCode.F4) || OVRInput.GetDown(OVRInput.Button.Start)  && !ToolController.ToolIsCurrentlyRunning && !PauseAndGUIBehaviour.isToolMenu && !ToolController.ToolControllerInterfaceIsCurrentlyRunning)
            {
                PauseAndGUIBehaviour.isSettingsMenu = !PauseAndGUIBehaviour.isSettingsMenu;
            }










        }

        IEnumerator ShowNotification(string message, float delay)
        {

            GameObject.Find("Canvas").gameObject.transform.Find("Notification").Find("NotificationText").GetComponent<UnityEngine.UI.Text>().text = message;
            if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
                GameObject.Find("Canvas_Oculus").gameObject.transform.Find("Notification").Find("NotificationText").GetComponent<UnityEngine.UI.Text>().text = message;

            //notificationText.GetComponent<UnityEngine.UI.Text>().enabled = true;
            GameObject.Find("Canvas").gameObject.transform.Find("Notification").gameObject.SetActive(true);
            if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
                GameObject.Find("Canvas_Oculus").gameObject.transform.Find("Notification").gameObject.SetActive(true);



            yield return new WaitForSeconds(delay);
            //notificationText.GetComponent<UnityEngine.UI.Text>().enabled = false;
            GameObject.Find("Canvas").gameObject.transform.Find("Notification").gameObject.SetActive(false);
            if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
                GameObject.Find("Canvas_Oculus").gameObject.transform.Find("Notification").gameObject.SetActive(false);

        }

        void UpdateStateView()
        {
            
            switch (sv)
            {
                case StateSingleton.StateView.MODE2D:
                    Mode2DWalking.SetActive(true);
                    Mode2DFlight.SetActive(true);
                    Mode2DDrone.SetActive(true);
                    Mode3DVPWalking.SetActive(false);
                    Mode3DVPFlight.SetActive(false);
                    Mode3DVPDrone.SetActive(false);
                    ModeOculusWalking.SetActive(false);
                    ModeOculusFlight.SetActive(false);
                    ModeOculusDrone.SetActive(false);
                    OculusTouchLeft.SetActive(false);
                    OculusTouchRight.SetActive(false);
                    Canvas.SetActive(true);
                    if (CanvasL!=null && CanvasR!=null)
                    {
                        Destroy(CanvasL);
                        Destroy(CanvasR);
                    }
                    if (CanvasOculus != null)
                        Destroy(CanvasOculus);
                    FlightModeObj.GetComponent<TopCameraControlller>().camera = Mode2DFlight;
                    DroneModeObj.GetComponent<DroneModeController>().camera = Mode2DDrone;

                    //Set FPSController exception
                    WalkingModeObj.GetComponent<FirstPersonController>().isOculusTouch = false;
                    //WalkingModeObj.GetComponent<FirstPersonController>().LeftOculusTouch = null;

                    //SetDisplays
                    foreach (GameObject go in new GameObject[] { Mode2DWalking, Mode2DFlight, Mode2DDrone })
                    {
                        go.GetComponent<Camera>().targetDisplay = 0;
                    }
                    break;
                case StateSingleton.StateView.MODE2D_PLUS_3DVP:
                    Mode2DWalking.SetActive(true);
                    Mode2DFlight.SetActive(true);
                    Mode2DDrone.SetActive(true);
                    Mode3DVPWalking.SetActive(true);
                    Mode3DVPFlight.SetActive(true);
                    Mode3DVPDrone.SetActive(true);
                    ModeOculusWalking.SetActive(false);
                    ModeOculusFlight.SetActive(false);
                    ModeOculusDrone.SetActive(false);
                    OculusTouchLeft.SetActive(false);
                    OculusTouchRight.SetActive(false);
                    if (CanvasOculus != null)
                        Destroy(CanvasOculus);
                    if (CanvasL==null && CanvasR==null)
                    {
                        CanvasL = (GameObject)Instantiate(Canvas);
                        CanvasL.name = "CanvasL";
                        CanvasL.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                        foreach (Transform tr in CanvasL.GetComponentsInChildren<Transform>(true))
                            if (tr.gameObject!=CanvasL && tr.parent.gameObject == CanvasL)
                                tr.localScale = new Vector3(0.5f, 1, 1);


                        CanvasR = (GameObject)Instantiate(Canvas);
                        CanvasR.name = "CanvasR";
                        CanvasR.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                        foreach (Transform tr in CanvasR.GetComponentsInChildren<Transform>(true))
                            if (tr.gameObject != CanvasR && tr.parent.gameObject == CanvasR)
                                tr.localScale = new Vector3(0.5f, 1, 1);
                        //Canvas.SetActive(false);
                    }
                    FlightModeObj.GetComponent<TopCameraControlller>().camera = Mode3DVPFlight;
                    DroneModeObj.GetComponent<DroneModeController>().camera = Mode3DVPDrone;

                    if (Display.displays.Length>1) Display.displays[1].Activate();

                    //Set Displays
                    foreach (GameObject go in new GameObject[] {Mode2DWalking, Mode2DFlight, Mode2DDrone })
                    {
                        go.GetComponent<Camera>().targetDisplay = 0;
                    }

                    //Display.displays[1].Activate();

                    foreach (GameObject go in new GameObject[] {Mode3DVPWalking, Mode3DVPFlight, Mode3DVPDrone})
                    {
                        foreach (Camera c in go.GetComponentsInChildren<Camera>())
                        {
                            c.targetDisplay = 1;
                        }
                    }

                    //Set FPSController exception
                    WalkingModeObj.GetComponent<FirstPersonController>().isOculusTouch = false;
                    //WalkingModeObj.GetComponent<FirstPersonController>().LeftOculusTouch = null;

                    break;
                case StateSingleton.StateView.MODE2D_PLUS_OCULUS:
                    Mode2DWalking.SetActive(false);
                    Mode2DFlight.SetActive(false);
                    Mode2DDrone.SetActive(false);
                    Mode3DVPWalking.SetActive(false);
                    Mode3DVPFlight.SetActive(false);
                    Mode3DVPDrone.SetActive(false);
                    ModeOculusWalking.SetActive(true);
                    ModeOculusFlight.SetActive(true);
                    ModeOculusDrone.SetActive(true);
                    OculusTouchLeft.SetActive(true);
                    OculusTouchRight.SetActive(true);
                    if (CanvasL != null || CanvasR != null)
                    {
                        Destroy(CanvasL);
                        Destroy(CanvasR);
                    }
                    if (CanvasOculus==null)
                    {
                        CanvasOculus = (GameObject)Instantiate(Canvas);
                        CanvasOculus.name = "Canvas_Oculus";
                        
                        CanvasOculus.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;

                        if (ExperimentalOculusUI)
                        {
                            //CanvasOculus.transform.SetParent(OculusTouchLeft.transform);

                            CanvasOculus.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
                            //CanvasOculus.transform.localScale *= oculusCanvasScale * 0.001f;
                        }


                        BoxCollider coll;
                        foreach (Transform tr in CanvasOculus.GetComponentsInChildren<Transform>(true))
                        {
                            if (tr.gameObject.GetComponent<Button>() != null)
                            {
                              //  print(tr.gameObject.name);
                                coll = tr.gameObject.AddComponent<BoxCollider>() as BoxCollider;
                                coll.isTrigger = true;
                                coll.size = new Vector3(
                                    tr.gameObject.GetComponent<RectTransform>().sizeDelta.x,
                                    tr.gameObject.GetComponent<RectTransform>().sizeDelta.y,
                                    0.01f
                                    );
                            }

                        }
                    }
                    FlightModeObj.GetComponent<TopCameraControlller>().camera = ModeOculusFlight;
                    DroneModeObj.GetComponent<DroneModeController>().camera = ModeOculusDrone;

                    //Set FPSController exception
                    WalkingModeObj.GetComponent<FirstPersonController>().isOculusTouch = true;
                    //WalkingModeObj.GetComponent<FirstPersonController>().LeftOculusTouch = OculusTouchLeft;
                    
                    // Set Oculus canvas
                    //FlightModeObj.GetComponent<VirtualMeter>().oculusToolInfo = CanvasOculus.transform.Find("Current Tool").gameObject; 
                    //DroneModeObj.GetComponent<VirtualMeter>().oculusToolInfo = CanvasOculus.transform.Find("Current Tool").gameObject;
                    //WalkingModeObj.GetComponent<VirtualMeter>().oculusToolInfo = CanvasOculus.transform.Find("Current Tool").gameObject;
                    
                    break;
                
                case StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS:

                    Mode2DWalking.SetActive(false);
                    Mode2DFlight.SetActive(false);
                    Mode2DDrone.SetActive(false);
                    Mode3DVPWalking.SetActive(true);
                    Mode3DVPFlight.SetActive(true);
                    Mode3DVPDrone.SetActive(true);
                    ModeOculusWalking.SetActive(true);
                    ModeOculusFlight.SetActive(true);
                    ModeOculusDrone.SetActive(true);
                    OculusTouchLeft.SetActive(true);
                    OculusTouchRight.SetActive(true);
                    if (CanvasL == null || CanvasR == null)
                    {
                        CanvasL = (GameObject)Instantiate(Canvas);
                        CanvasL.name = "CanvasL";
                        CanvasL.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                        CanvasR = (GameObject)Instantiate(Canvas);
                        CanvasR.name = "CanvasR";
                        CanvasR.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                        //Canvas.SetActive(false);
                    }
                    FlightModeObj.GetComponent<TopCameraControlller>().camera = Mode3DVPFlight;
                    DroneModeObj.GetComponent<DroneModeController>().camera = Mode3DVPDrone;

                    //SetDisplays
                    foreach (GameObject go in new GameObject[] { Mode3DVPWalking, Mode3DVPFlight, Mode3DVPDrone })
                    {
                        go.GetComponent<Camera>().targetDisplay = 0;
                    }

                    //Set FPSController exception
                    WalkingModeObj.GetComponent<FirstPersonController>().isOculusTouch = true;
                    //WalkingModeObj.GetComponent<FirstPersonController>().LeftOculusTouch = OculusTouchLeft;
                    break;
            }
            
        }

   
        }

    }

