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

public class Char_VR_button : Abstract_VR_button {
    public char char_key = (char)0;
    public List<TextMesh> low_cap_sym = new List<TextMesh>();

    protected new void Start()
    {
        base.Start();
        if (char_key == (char)0)
        {
            if (low_cap_sym.Count == 0)
                low_cap_sym.Add(GetComponentInChildren<TextMesh>());
            char_key = low_cap_sym[0].text.ToCharArray()[0];
        }
        //this is only to manage the return char, which apparently is not assignable from the unity GUI
        else if (char_key == '⏎')
        {
            char_key = '\n';
        }
    }

    public override void onPress()
    {
        //input to the textBox
        keyboardManager.type_char(char_key);
    }

    public void switchToLow()
    {
        if (low_cap_sym == null || low_cap_sym.Count < 3)
            return;
        low_cap_sym[1].gameObject.SetActive(false);
        low_cap_sym[2].gameObject.SetActive(false);
        low_cap_sym[0].gameObject.SetActive(true);
        char_key = low_cap_sym[0].text.ToCharArray()[0];
    }

    public void switchToCapital()
    {
        if (low_cap_sym == null || low_cap_sym.Count < 3)
            return;
        low_cap_sym[0].gameObject.SetActive(false);
        low_cap_sym[2].gameObject.SetActive(false);
        low_cap_sym[1].gameObject.SetActive(true);
        char_key = low_cap_sym[1].text.ToCharArray()[0];
    }

    public void switchToSymbol()
    {
        if (low_cap_sym == null || low_cap_sym.Count < 3)
            return;
        low_cap_sym[0].gameObject.SetActive(false);
        low_cap_sym[1].gameObject.SetActive(false);
        low_cap_sym[2].gameObject.SetActive(true);
        char_key = low_cap_sym[2].text.ToCharArray()[0];
    }
}
