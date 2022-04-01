using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatePositionSingleton : MonoBehaviour {
	
	int rotationGap;
	
	// Update is called once per frame
	void Update () {
		
        PositionSingleton.playerContinousPosition = transform.position;

		PositionSingleton.playerContinousRotation = transform.eulerAngles;
		
	}
}
