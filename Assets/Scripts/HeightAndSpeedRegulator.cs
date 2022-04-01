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
       // cc = GetComponent<CharacterController>();
       
	}
	
	// Update is called once per frame
	void Update () {
    /*
     *if (Input.GetKey(KEY_HEIGHT_REGULATOR_MINUS) && cc.height > MIN_HEIGHT)
        {
            cc.height -= step;
            transform.position += Vector3.down * Time.deltaTime*SPEED_OFFSET;
        }
        else if (Input.GetKey(KEY_HEIGHT_REGULATOR_PLUS) && cc.height < MAX_HEIGHT)
        {
            cc.height += step;
            transform.position += Vector3.up * Time.deltaTime * SPEED_OFFSET*SPEED_OFFSET_PERCENT;
        }
        */
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

                //	StartCoroutine(BlinkText(speedTextPanel);

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
