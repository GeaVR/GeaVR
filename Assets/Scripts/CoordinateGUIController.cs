using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoordinateGUIController : MonoBehaviour {

    public Text northing, altitude, orientation, elevation;
    public Image compass;
	float n, rotationGap;
	
	void Update () {
        //Set GUI on screen
    //    orientation.text =  PositionSingleton.playerContinousRotation.y.ToString("0") + "°N";
		if (PositionSingleton.playerContinousRotation.y>=180) rotationGap=-180;
			else if (PositionSingleton.playerContinousRotation.y <180) rotationGap=180;

        // rotationgap is not necessary if obj are rotated properly

        rotationGap = 0;
        
        n = PositionSingleton.playerContinousRotation.y+rotationGap;

        //scale n from 1 to 360
        float n1 = (359 * (n / 360)) + 1;

       

        orientation.text = "Heading: "+ n1.ToString("0") + "°N";
        northing.text = "(" +PositionSingleton.playerRealPosition.z.ToString("0.000000")+", "+ PositionSingleton.playerRealPosition.x.ToString("0.000000") + ")";
        altitude.text = "Altitude: " + PositionSingleton.playerRealPosition.y.ToString("0.000")+" (m)";
        compass.transform.localEulerAngles = new Vector3(0, 0, PositionSingleton.playerContinousRotation.y);
        elevation.text = "Navigation Height: " + PositionSingleton.playerRealElevation.ToString("0.000");

    }
}
