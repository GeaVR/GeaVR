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

public class AddToOculusCanvas : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine("addToOculusCanvas");
	}
	

    private IEnumerator addToOculusCanvas()
    {
        while (GameObject.Find("Canvas_Oculus") == null)
        {
            yield return new WaitForEndOfFrame();
        }
            transform.parent = GameObject.Find("Canvas_Oculus").transform;
            transform.localPosition = new Vector3(-25, -350, -175);
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.localScale = new Vector3(40, 40, 1);
    }
	
}
