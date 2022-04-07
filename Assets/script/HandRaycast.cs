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

public class HandRaycast : MonoBehaviour {

    private int m_RayLength = 100;

    public GameObject handController;
    public OVRInput.RawButton controller_button;
    public OVRInput.RawButton selection_text_button;
    
    private float waitTime = 0.2f;
    private float timer = 0.0f;
    private Abstract_VR_button lastCharPressed = null;


    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position, transform.forward, out hit, m_RayLength))
        {
            if (hit.collider.gameObject.GetComponent<Abstract_VR_button>())
            {
                //this is to highlight the pointed button
                hit.collider.gameObject.GetComponent<Abstract_VR_button>().keepSelected = .05f;

                if (OVRInput.GetDown(controller_button))
                {
                    hit.collider.gameObject.GetComponent<Abstract_VR_button>().onPress();
                   
                }
            }
            else if (hit.collider.gameObject.GetComponent<VRKeyboard_Text>() && OVRInput.GetUp(controller_button))
            {
                hit.collider.gameObject.GetComponent<VRKeyboard_Text>().OnSelect();
                hit.collider.gameObject.GetComponent<VRKeyboard_Text>().GoOnLastChar();
            }
            else if (hit.collider.gameObject.GetComponent<VRKeyboard_Text>() && OVRInput.GetUp(selection_text_button))
            {
                hit.collider.gameObject.GetComponent<VRKeyboard_Text>().OnSelect();
            }
        }

    }
}
