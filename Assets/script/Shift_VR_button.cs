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

public class Shift_VR_button : Abstract_VR_button {
    public List<GameObject> buttons_to_switch = new List<GameObject>();
    public bool doubleClick = false;

    //IMPORTANT: if the symbol mode of the keyboard is active, then pressing the shift has no effects
    public override void onPress()
    {
        //switch to upper case
        if (buttons_to_switch[0].GetComponent<Char_VR_button>().low_cap_sym[0].gameObject.activeSelf)
        {
            foreach (GameObject button in buttons_to_switch)
            {
                button.GetComponent<Char_VR_button>().switchToCapital();
            }
        }
        //switch to lower case
        else if (buttons_to_switch[0].GetComponent<Char_VR_button>().low_cap_sym[1].gameObject.activeSelf)
        {
            foreach (GameObject button in buttons_to_switch)
            {
                button.GetComponent<Char_VR_button>().switchToLow();
            }
        }
        StartCoroutine("startDoubleClickTimer");
    }
    
    private IEnumerator startDoubleClickTimer()
    {
        doubleClick = true;
        yield return new WaitForSeconds(2);
        doubleClick = false;
    }
}
