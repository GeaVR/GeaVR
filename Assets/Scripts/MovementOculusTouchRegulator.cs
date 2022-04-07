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

using UnityStandardAssets.Characters.FirstPerson;
[RequireComponent(typeof(FirstPersonController))]

public class MovementOculusTouchRegulator : MonoBehaviour {
    // Use this for initialization
    private FirstPersonController fpc;
    public GameObject RightOculusTouch;
    void Start () {
        fpc = GetComponent<FirstPersonController>();
	}
	
	// Update is called once per frame
	void Update () {
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS) {
            Vector2 move2d = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            Vector3 desiredMove = Vector3.forward * move2d.y + Vector3.right * move2d.x;
            fpc.desiredMove = RightOculusTouch.transform.TransformDirection(desiredMove);
        }
	}
}
