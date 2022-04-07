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
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class MyOVRTrackedRemote : MonoBehaviour {
    public GameObject target, WalkingModeObj, FlightModeObj, DroneModeObj;
    public GameObject targetCamera, WalkingModeObjCamera, FlightModeObjCamera, DroneModeObjCamera;
    public GameObject Anchor, WalkingModeAnchor, FlightModeAnchor, DroneModeAnchor;
    public StateSingleton.StateMode sm = StateSingleton.StateMode.UNSET;
    private Vector3 startOffsetPosition;
    public Vector3 startOffsetRotation;
    private Vector3 centerOfCamera;
    private bool lineAppear = true;

    private RaycastHit hit;
    private bool preventMultiTrigger = false;
    private GameObject hoveredObject = null;

    public OVRInput.Controller controller;
    public GameObject pointer;
    public float RayOffset;

    // Use this for initialization
    void OnEnable () {
        centerOfCamera = new Vector3(targetCamera.GetComponent<Camera>().pixelWidth/2, targetCamera.GetComponent<Camera>().pixelWidth/2, 0);
        startOffsetRotation = targetCamera.transform.rotation.eulerAngles;



        ChangeState();
       // Debug.Log("sono in enable");
        ChangeState();
    }
	
	// Update is called once per frame
	void FixedUpdate () {

       // Debug.Log("Chiamo fixed update controller: " + controller);

        if (sm != StateSingleton.stateMode)
            ChangeState();
        
        /*
                if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > 0.8 && OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.8)
                    ChangeState();
         
        
                       if (controller == OVRInput.Controller.RTouch)
                       {

                           RaycastHit hit;
                           if (Physics.Raycast(transform.position, transform.forward, out hit, 100000))
                           {

                               if (hit.collider.gameObject.GetComponent<ButtonsPauseMenu>() != null)
                               {
                                   hit.collider.gameObject.GetComponent<ButtonsPauseMenu>().OculusTriggerButtonOn();
                               }

                               if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.90f && hit.collider.gameObject.GetComponent<Button>() != null)
                               {


                                   if (hit.collider.gameObject.GetComponent<ButtonsPauseMenu>() != null)
                                   {
                                       hit.collider.gameObject.GetComponent<ButtonsPauseMenu>().OculusTriggerButton();
                                   }

                                   //  hit.collider.gameObject.GetComponent<Button>().onClick.Invoke(); // invoke built in UI

                                   if (!preventMultiTrigger)
                                   {
                                       hit.collider.gameObject.GetComponent<Button>().onClick.Invoke(); // invoke built in UI
                                       preventMultiTrigger = true;
                                   }

                               }
                               if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) <= 0.90f)
                               {
                                   preventMultiTrigger = false;
                               }





                           }
                       }
                       
                       else if (lineAppear && controller == OVRInput.Controller.RTouch)
                       {
                           //GetComponent<LineRenderer>().enabled = false;
                           //lineAppear = false;
                       }
                       */

                       
    }

    void Update()
    {
        //Debug.Log("Chiamo update controller: "+controller);



        if (controller == OVRInput.Controller.RTouch)
        {

            GetComponent<LineRenderer>().SetPosition(0, transform.position + transform.forward * RayOffset);
            GetComponent<LineRenderer>().SetPosition(1, transform.position + transform.forward * RayOffset * 2.0f);

            if (Physics.Raycast(transform.position, transform.forward, out hit, 100000))
            {
                GetComponent<LineRenderer>().SetPosition(2, hit.point);
                pointer.transform.position = hit.point;

                if (hit.collider.gameObject.GetComponent<ButtonsPauseMenu>() != null)
                {
                    hit.collider.gameObject.GetComponent<ButtonsPauseMenu>().OculusTriggerButtonOn();
                }

                if (hoveredObject == null && hit.collider.gameObject.GetComponent<Button>() != null)
                {
                    // execute pointereEnter event on new button
                    ExecuteEvents.Execute<IPointerEnterHandler>(hit.collider.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);

                    hoveredObject = hit.collider.gameObject;
                }
                else if (hoveredObject != hit.collider.gameObject)
                {
                    // execute pointereExit event on old button
                    ExecuteEvents.Execute<IPointerExitHandler>(hoveredObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
                    // execute pointereEnter event on new button
                    ExecuteEvents.Execute<IPointerEnterHandler>(hit.collider.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
                    hoveredObject = hit.collider.gameObject;
                }

                if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.90f && hit.collider.gameObject.GetComponent<Button>() != null)
                {
                    if (hit.collider.gameObject.GetComponent<ButtonsPauseMenu>() != null)
                    {
                        hit.collider.gameObject.GetComponent<ButtonsPauseMenu>().OculusTriggerButton();
                    }

                    if (!preventMultiTrigger)
                    {
                        hit.collider.gameObject.GetComponent<Button>().onClick.Invoke(); // invoke built in UI
                        preventMultiTrigger = true;
                    }
                }
                if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) <= 0.90f)
                {
                    preventMultiTrigger = false;
                }

                // move visual aid (sphere) to pointer position 
                //GetComponent<LineRenderer>().SetPosition(2, transform.position + transform.forward * 10000);
                //pointer.transform.position = transform.position + transform.forward * 10000;
            }
            else
            {
                if (hoveredObject != null) // if no longer hovering over an object
                {
                    ExecuteEvents.Execute<IPointerExitHandler>(hoveredObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler); // invoke on exit
                   // ExecuteEvents.Execute<IPointerExitHandler>(hit.collider.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler); // invoke on exit
                    hoveredObject = null;
                }

                GetComponent<LineRenderer>().SetPosition(2, transform.position + transform.forward * 10000);
                pointer.transform.position = transform.position + transform.forward * 10000;


                //                GetComponent<LineRenderer>().SetPosition(2, transform.forward * 10000);
                //              pointer.transform.position = transform.forward * 10000;
            }
        }
    }

    public void Initialize(float y, float z, float optionalYRotation = 0f)
    {
        float x;
        //y += targetCamera.GetComponent<Camera>().ScreenToWorldPoint(centerOfCamera).y;
        //z += targetCamera.GetComponent<Camera>().ScreenToWorldPoint(centerOfCamera).z;
        if (controller == OVRInput.Controller.LTouch || controller == OVRInput.Controller.LTrackedRemote)
            x = -3;
        else if (controller == OVRInput.Controller.RTouch || controller == OVRInput.Controller.RTrackedRemote)
            x = 3;
        else x = 0;

        Vector3 v = new Vector3(x, y, z);
        //startOffsetPosition = targetCamera.transform.TransformDirection(v.normalized) + v;
        startOffsetPosition = targetCamera.transform.position;
        //startOffsetRotation = new Vector3(optionalYRotation, 0, 0);
    }
    
    void ChangeState()
    {
        sm = StateSingleton.stateMode;
       // Debug.Log("Fixing Joypad Position " + sm);

        switch (sm)
        {
            case StateSingleton.StateMode.WALKING:
                target = WalkingModeObj;
                targetCamera = WalkingModeObjCamera;                

                transform.SetParent(WalkingModeAnchor.transform, false);

                Initialize(9, 10);
                break;
            case StateSingleton.StateMode.FLIGHT:
                Debug.Log("inizoo flight in intitializsw");

                target = FlightModeObj;
                targetCamera = FlightModeObjCamera;
                Debug.Log("setto i targert flight in intitializsw");

                transform.SetParent(FlightModeAnchor.transform, false);
                Debug.Log("transfor flight in intitializsw");

                Initialize(0, 9, 90);
                Debug.Log("fineializsw");

                break;
            case StateSingleton.StateMode.DRONE:
                target = DroneModeObj;
                targetCamera = DroneModeObjCamera;

                transform.SetParent(DroneModeAnchor.transform, false);

                Initialize(-2, 10);
                break;
        }
    }



}