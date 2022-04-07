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

public class DroneModeController : MonoBehaviour {

    public float SPEED = 10, SPEEDROT=1, MAX_ROTATION_ANGLE = 75;
    public int SPEEDZ = 100;
    public float VRSPEEDROT = 1.0f;
    public int STARTHEIGHT = 10000;
    public int MAXHEIGHT = 10000;
    public int MINHEIGHT = 100;
    public float ROTATIONSMOOTHINGSCALINGFACTOR = 3.0f, ROTATIONSMOOTHINGTIME = 0.5F, ROTATIONCAP = 360.0f, joystickDeadzone = 0.3f;
    public KeyCode KEYUP = KeyCode.Z, KEYDOWN = KeyCode.X;
    public KeyCode KEY_SPEED_REGULATOR_PLUS = KeyCode.M, KEY_SPEED_REGULATOR_MINUS = KeyCode.N;    
    public GameObject camera, player;
    public bool isPPCCrunning = false;
    public bool isMTPCrunning = false;
    public bool isRWTCrunning = false;
    public float distance = 0;
    LineRenderer lr;

    public GameObject pos;

    public GameObject OculusTouchRight, OculusTouchLeft;
	// Use this for initialization
	void Start () {
      //  transform.position = new Vector3(transform.position.x, STARTHEIGHT, transform.position.z);
    }
  
    // Update is called once per frame
    void FixedUpdate () {

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS && OculusTouchRight.GetComponent<LineRenderer>().enabled == false)
            OculusTouchRight.GetComponent<LineRenderer>().enabled = true;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 desiredDirection = (new Vector3(x, 0, z));
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
        {
            // position control
            // horizontal
            Vector2 move2d = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            desiredDirection = Vector3.forward * move2d.y + Vector3.right * move2d.x;

            desiredDirection = OculusTouchRight.transform.TransformDirection(desiredDirection);

            // project onto ground plane to prevent vertical movement
            // comment the next line to have functionality like Google earth
            // desiredDirection = Vector3.ProjectOnPlane(desiredDirection, Vector3.up).normalized; 

            //vertical
            move2d = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            MoveVertical(SPEEDZ*move2d.y);

            //Rotate view 
            if (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.3f && isRWTCrunning == false)
            {
                StartCoroutine(RotateWithTriggerCoroutine());
            }
            else
            {
                if (Mathf.Abs(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x) > joystickDeadzone)
                {
                        Vector2 rotate2D = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
                        transform.Rotate(0.0f, rotate2D.x * VRSPEEDROT, 0.0f);
                }
            }
        }
        else
        {
            desiredDirection = transform.TransformDirection(desiredDirection);
        }

        transform.position += desiredDirection*SPEED*Time.deltaTime;        

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down + 0.5f*Vector3.forward, out hit, 1000))
        {


            if (Vector3.Distance(hit.point, transform.position) < 2 && !isPPCCrunning && !isMTPCrunning)
            {
                StartCoroutine(PreventPlaneCrashCoroutine(transform.position));
            }
        }


        if (StateSingleton.stateView != StateSingleton.StateView.MODE2D_PLUS_OCULUS && StateSingleton.stateView != StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
        {

            if (Input.GetKey(KEYDOWN) )
            { 
                MoveVertical(SPEEDZ);
            }
            else if (Input.GetKey(KEYUP) )
            {
                MoveVertical(-SPEEDZ);
            }

            float rotx = Input.GetAxis("Mouse X");
            float roty = -Input.GetAxis("Mouse Y");
            float futureAngle = ((transform.eulerAngles.x + roty > 0) ? (transform.eulerAngles.x + roty) : (360 + transform.eulerAngles.x + roty))%360;
            transform.eulerAngles += (
                (futureAngle >0 && futureAngle < MAX_ROTATION_ANGLE) ||
                (futureAngle > 360 - MAX_ROTATION_ANGLE && futureAngle < 360)
                ) ? new Vector3(roty, rotx, 0)*Time.deltaTime*SPEEDROT : Vector3.zero;


            if (Input.GetKey(KEY_SPEED_REGULATOR_MINUS))
            {
                SPEED -= (int)(200 * Time.deltaTime);
                SPEED = (int)Mathf.Clamp((float)SPEED, 10f, 1000f);
            }
            else if (Input.GetKey(KEY_SPEED_REGULATOR_PLUS))
            {
                SPEED += (int)(200 * Time.deltaTime);
                SPEED = (int)Mathf.Clamp((float)SPEED, 10f, 1000f);

            }
        }

        PositionSingleton.playerContinousPosition = transform.position;
        PositionSingleton.playerContinousRotation = transform.eulerAngles;
/*
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS && !PauseAndGUIBehaviour.isPause && !PauseAndGUIBehaviour.isModeMenu)
        {
            lr = OculusTouchRight.GetComponent<LineRenderer>();
            LinePointer();
        }
 */
        
    }

    void MoveVertical(float step)
    {


            transform.position += Vector3.up * step * Time.deltaTime;
    }

    IEnumerator PreventPlaneCrashCoroutine(Vector3 point)
    {

        isPPCCrunning = true;
        Vector3 target = point + Vector3.up * 1 + transform.TransformDirection(Vector3.back)*1;

        float frac=0;
        while(frac<1)
        {

            frac = Vector3.Distance(point, transform.position) / Vector3.Distance(point, target) + 0.05f ;


            transform.position=Vector3.Lerp(point, target, frac);
            yield return new WaitForEndOfFrame();
        }
        isPPCCrunning = false;
    }

    public void LinePointer()
    {

        /*
        Vector3 endPoint = OculusTouchRight.transform.position + OculusTouchRight.transform.TransformDirection(Vector3.forward * 10000);
        bool isEndPointValid = false;
        RaycastHit hit;
        if (Physics.Raycast(OculusTouchRight.transform.position + OculusTouchRight.transform.forward, OculusTouchRight.transform.TransformDirection(Vector3.forward), out hit, 1000000))
        {
            endPoint = hit.point;
            isEndPointValid = true;
        }

        if (
            OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.3f 
            && isEndPointValid && !isMTPCrunning
            )
            StartCoroutine(MoveToPointCoroutine(endPoint,transform.eulerAngles.x));


        lr.SetPosition(0, OculusTouchRight.transform.position);
        if (!isMTPCrunning || !isRWTCrunning) lr.SetPosition(1, endPoint);
        distance = Vector3.Distance(OculusTouchRight.transform.position, endPoint);
        
    */

    }

    IEnumerator MoveToPointCoroutine(Vector3 target, float minAngle)
    {
  
        float tempWidth = OculusTouchRight.GetComponent<LineRenderer>().startWidth;
        Vector3 start = transform.position;
        isMTPCrunning = true;
        float frac = 0, angle;
        float startAngle = OculusTouchRight.transform.eulerAngles.x;
        startAngle = (startAngle > 180) ? (180 - startAngle) : startAngle;
        while (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.3f)
        {

            transform.position = Vector3.Lerp(start, target, frac);
            angle = OculusTouchRight.transform.eulerAngles.x - startAngle;
            angle = (angle > 180) ? (180 - angle) : angle;
            frac += Mathf.Clamp(angle, -45, 45)*0.002f;
            //frac += (ClampAngle(OculusTouchRight.transform.eulerAngles.x - minAngle - 40, -30, 30)*0.01f);
            frac = Mathf.Clamp(frac, 0f, 0.8f);
            if (OculusTouchRight.GetComponent<LineRenderer>().startWidth < 10 * tempWidth)
                OculusTouchRight.GetComponent<LineRenderer>().startWidth += 0.1f;
            yield return new WaitForEndOfFrame();
        }

        isMTPCrunning = false;
        while(OculusTouchRight.GetComponent<LineRenderer>().startWidth>tempWidth+0.1f)
        {
            OculusTouchRight.GetComponent<LineRenderer>().startWidth -= 0.1f;
            yield return new WaitForEndOfFrame();
        }
        OculusTouchRight.GetComponent<LineRenderer>().startWidth = tempWidth;
     
    }

    IEnumerator RotateWithTriggerCoroutine()
    {
        isRWTCrunning = true;
        //float angle = 0;
        //float startOculusTouchAngle = OculusTouchRight.transform.eulerAngles.y;
        //startOculusTouchAngle = (startOculusTouchAngle > 180) ? (180 - startOculusTouchAngle) : startOculusTouchAngle;

        float startAngle = transform.eulerAngles.y;
        //startAngle = (startAngle > 180) ? (180 - startAngle) : startAngle;

        float angleToSet = 0;
        float diffAngle = 0;
        float prevDiffAngle = 0;
        Vector3 startTouchVector = transform.worldToLocalMatrix * OculusTouchRight.transform.forward;

        float smoothingVelocity = 0.0F;

        while (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.3f)
        {
            print(diffAngle);            
            angleToSet = startAngle - diffAngle;
            angleToSet = (angleToSet < 0) ? (360 + angleToSet) : angleToSet;

            transform.eulerAngles = new Vector3(transform.eulerAngles.x, angleToSet, transform.eulerAngles.z);

            //Vector3 currentTouchVector = OculusTouchRight.transform.forward;
            //currentTouchVector = new Vector3(currentTouchVector.x, 0.0f, currentTouchVector.z).normalized;
            
            // interpolate to smooth flickering due to input noise
            prevDiffAngle = diffAngle;
            float targetAngle = Vector3.SignedAngle(startTouchVector, transform.worldToLocalMatrix * OculusTouchRight.transform.forward, Vector3.up);
            diffAngle = Mathf.SmoothDampAngle(prevDiffAngle, targetAngle * ROTATIONSMOOTHINGSCALINGFACTOR, ref smoothingVelocity, ROTATIONSMOOTHINGTIME);
            
            //angle = OculusTouchRight.transform.eulerAngles.y - startOculusTouchAngle;
            diffAngle = (diffAngle > 180) ? (180 - diffAngle) : diffAngle;
            diffAngle = Mathf.Clamp(diffAngle, -ROTATIONCAP, ROTATIONCAP);
            yield return new WaitForEndOfFrame();
        }
        isRWTCrunning = false;
        print("Rotated");
    }

  
}
