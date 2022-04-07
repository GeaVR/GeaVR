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
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

public class HeightAndSpeedRegulator : MonoBehaviour {

    public KeyCode KEY_HEIGHT_REGULATOR_PLUS = KeyCode.Z;
    public KeyCode KEY_HEIGHT_REGULATOR_MINUS = KeyCode.X;
    public KeyCode KEY_SPEED_REGULATOR_PLUS = KeyCode.M, KEY_SPEED_REGULATOR_MINUS = KeyCode.N;
    public int MAX_HEIGHT = 200, MIN_HEIGHT = 50;
    public int step = 10, SPEED_OFFSET = 200;
    public float SPEED_OFFSET_PERCENT = 0.2f;
   // CharacterController cc;
    CustomFPCharacter fpc;
    DroneModeController dpc;
    TopCameraControlller tpc;

    public Text speedText;

    // Use this for initialization
    void Start () {
       
	}
	
	// Update is called once per frame
	void Update () {
   
        if (Input.GetKey(KEY_SPEED_REGULATOR_MINUS))
        {
            DecreaseSpeed();

        }
        else if (Input.GetKey(KEY_SPEED_REGULATOR_PLUS))
        {
            IncreaseSpeed();

        }
    }

    public void IncreaseSpeed()
    {

        switch (StateSingleton.stateMode)
        {
            case StateSingleton.StateMode.WALKING:
                fpc = GetComponent<CustomFPCharacter>();
                fpc.m_WalkSpeed += SPEED_OFFSET * Time.deltaTime;
                fpc.m_WalkSpeed = Mathf.Clamp(fpc.m_WalkSpeed, 1f, SPEED_OFFSET_PERCENT * fpc.m_RunSpeed);
                break;
            case StateSingleton.StateMode.FLIGHT:
                tpc = GetComponent<TopCameraControlller>();
                tpc.SPEEDXZ += (int)(SPEED_OFFSET * 5 * Time.deltaTime);
                tpc.SPEEDXZ = (int)Mathf.Clamp((float)tpc.SPEEDXZ, 1f, 1000f);

                break;
            case StateSingleton.StateMode.DRONE:
                dpc = GetComponent<DroneModeController>();
                      
                dpc.SPEED += (int)(SPEED_OFFSET * 5 * Time.deltaTime);
                dpc.SPEED = (int)Mathf.Clamp((float)dpc.SPEED, 1f, 1000f);
                break;
        }

       
        speedText.text = "Increase Speed";
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("Speed").gameObject.transform.Find("SpeedPanel").gameObject.transform.Find("Elevation").GetComponent<Text>().text = speedText.text;

    }

    public void DecreaseSpeed()
    {
        switch (StateSingleton.stateMode)
        {
            case StateSingleton.StateMode.WALKING:
                fpc = GetComponent<CustomFPCharacter>();
                fpc.m_WalkSpeed -= SPEED_OFFSET * Time.deltaTime;
                fpc.m_WalkSpeed = Mathf.Clamp(fpc.m_WalkSpeed, 1f, SPEED_OFFSET_PERCENT * fpc.m_RunSpeed);
            break;
            case StateSingleton.StateMode.FLIGHT:
                tpc = GetComponent<TopCameraControlller>();
                tpc.SPEEDXZ -= (int)(SPEED_OFFSET*5 * Time.deltaTime);
                tpc.SPEEDXZ = (int)Mathf.Clamp((float)tpc.SPEEDXZ, 1f, 1000f);

                break;
            case StateSingleton.StateMode.DRONE:
                dpc = GetComponent<DroneModeController>();
                dpc.SPEED -= (int)(SPEED_OFFSET * 5 * Time.deltaTime);
                dpc.SPEED = (int)Mathf.Clamp((float)dpc.SPEED, 1f, 1000f);
            break;
    }

        speedText.text = "Decrease Speed";
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("Speed").gameObject.transform.Find("SpeedPanel").gameObject.transform.Find("Elevation").GetComponent<Text>().text = speedText.text;
          
    }
}
