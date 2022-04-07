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

public class TopCameraControlller : MonoBehaviour {

    public int SPEEDXZ = 10;
    public int SPEEDZ = 10;
    public int SPEEDROTATION = 10;
    public int STARTHEIGHT = 10000;
    public int MAXHEIGHT = 10000;
    public int MINHEIGHT = 100;
    public KeyCode KEYUP = KeyCode.Z, KEYDOWN = KeyCode.X;
    public KeyCode KEY_SPEED_REGULATOR_PLUS = KeyCode.M, KEY_SPEED_REGULATOR_MINUS = KeyCode.N;
    public float ROTATIONSMOOTHINGSCALINGFACTOR = 3.0f, ROTATIONSMOOTHINGTIME = 0.5F, ROTATIONCAP = 360.0f;
    public float VRSPEEDROT = 1.0f;
    public GameObject camera, player, OculusTouchRight;
    StateSingleton.StateMode previousState;


	// Use this for initialization
	void Start () {
        transform.position = new Vector3(transform.position.x, STARTHEIGHT, transform.position.z);
    
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKey(KEYDOWN) && transform.position.y> MINHEIGHT)
        {
            MoveVertical(-SPEEDZ);
        }
        else if (Input.GetKey(KEYUP) && transform.position.y < MAXHEIGHT) 
        {
            MoveVertical(SPEEDZ);
        }
        
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
        {
            Vector2 move2d = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            Vector3 desiredDirection = Vector3.forward * move2d.y + Vector3.right * move2d.x;

            desiredDirection = transform.TransformDirection(desiredDirection);

            move2d = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            MoveVertical(SPEEDZ * move2d.y);

            transform.position += desiredDirection * SPEEDXZ * Time.deltaTime;

            //Rotate view             
            Vector2 rotate2D = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            transform.Rotate(0.0f, rotate2D.x * VRSPEEDROT, 0.0f);
            
        }
        else
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 desiredDirection = transform.TransformDirection(new Vector3(x, 0, z) * SPEEDXZ);
            transform.position += desiredDirection * SPEEDXZ * Time.deltaTime;

            float rotx = Input.GetAxis("Mouse X");
            transform.eulerAngles += Vector3.up * rotx * SPEEDROTATION * Time.deltaTime;
        }

        PositionSingleton.playerContinousPosition = transform.position;
        PositionSingleton.playerContinousRotation = transform.eulerAngles;

        if (Input.GetKey(KEY_SPEED_REGULATOR_MINUS))
        {
            SPEEDXZ -= (int)(200 * Time.deltaTime);
            SPEEDXZ = (int)Mathf.Clamp((float)SPEEDXZ, 10f, 1000f);

        }
        else if (Input.GetKey(KEY_SPEED_REGULATOR_PLUS))
        {
            SPEEDXZ += (int)(200 * Time.deltaTime);
            SPEEDXZ = (int)Mathf.Clamp((float)SPEEDXZ, 10f, 1000f);

        }
    }

    void MoveVertical(float step)
    {
        transform.position += Vector3.up * step * Time.deltaTime;
    }
}
