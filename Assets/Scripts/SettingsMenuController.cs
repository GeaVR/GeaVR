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
