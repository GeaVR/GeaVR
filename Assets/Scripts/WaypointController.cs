using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaypointController : MonoBehaviour {

	public Text NorthingEtc; 
	// Use this for initialization
	void Start () {
		NorthingEtc.text= "(" + PositionSingleton.playerRealPosition.z.ToString("0.000000") + ", " + PositionSingleton.playerRealPosition.x.ToString("0.000000") + ", " + PositionSingleton.playerRealPosition.y.ToString("0.000") + ")"; ;
    }
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetKey(KeyCode.Escape))
		{

		}
		
	}
	
	void OnDestroy()
    {
        Debug.Log("OnDestroy1");
		        PauseAndGUIBehaviour.isGPS2 = false;

    }

}
