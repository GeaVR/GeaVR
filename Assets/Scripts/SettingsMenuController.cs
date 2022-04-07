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
using UnityEngine.SceneManagement;

public class SettingsMenuController : MonoBehaviour {


	public GameObject WalkingModeObj, FlightModeObj, DroneModeObj;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	public void QuitApplication()
	{
		Application.Quit();
	}


	public void LoadNewModel()
	{
		SceneManager.LoadScene("TerrainSelection");
		Time.timeScale = 1;
	}

	public void IncreaseSpeed()
	{
		switch (StateSingleton.stateMode)
		{
			case StateSingleton.StateMode.WALKING:
				Debug.Log("increase");
				WalkingModeObj.GetComponent<HeightAndSpeedRegulator>().IncreaseSpeed();
				
			break;
			case StateSingleton.StateMode.FLIGHT:
				FlightModeObj.GetComponent<HeightAndSpeedRegulator>().IncreaseSpeed();
				break;
			case StateSingleton.StateMode.DRONE:
				DroneModeObj.GetComponent<HeightAndSpeedRegulator>().IncreaseSpeed();
				break;
		}
	}


	public void DecreaseSpeed()
	{
		switch (StateSingleton.stateMode)
		{
			case StateSingleton.StateMode.WALKING:
				WalkingModeObj.GetComponent<HeightAndSpeedRegulator>().DecreaseSpeed();
			//	StartCoroutine(BlinkText(speedTextPanel);

				break;
			case StateSingleton.StateMode.FLIGHT:
				FlightModeObj.GetComponent<HeightAndSpeedRegulator>().DecreaseSpeed();
				break;
			case StateSingleton.StateMode.DRONE:
				DroneModeObj.GetComponent<HeightAndSpeedRegulator>().DecreaseSpeed();
				break;
		}
	}

/*
	IEnumerator BlinkText(GameObject t)
	{
		if (!t.activeSelf)
		{
			t.gameObject.SetActive(true);
			foreach (KeyCode key in keys)
			{
				while (Input.GetKey(key))
					yield return new WaitForSeconds(0.1f);
			}

			yield return new WaitForSeconds(2);
			t.gameObject.SetActive(isMeCoordinate);
		}
	}
	*/
}
