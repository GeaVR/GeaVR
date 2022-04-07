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

public class CoordinateGUIController : MonoBehaviour {

    public Text northing, altitude, orientation, elevation;
    public Image compass;
	float n, rotationGap;
	
	void Update () {
        //Set GUI on screen
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
