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

public abstract class Abstract_VR_button : MonoBehaviour {
    public KeyboardManager keyboardManager;

    public Material[] materials;    //this is to switch the color of the button while it is aimed by the ray
    public float keepSelected = 0;
    public int materialIndex = 0;
    protected Renderer renderer;

    protected void Start()
    {
        keyboardManager = GameObject.FindGameObjectWithTag("VRKeyboard").GetComponent<KeyboardManager>();
        renderer = GetComponent<Renderer>();
        renderer.enabled = true;
        renderer.sharedMaterial = materials[materialIndex];
    }

    protected void Update()
    {
        if (keepSelected > 0)
        {
            //switch material if it was not done before
            if (materialIndex == 0)
            {
                materialIndex = 1;
                renderer.sharedMaterial = materials[materialIndex];
            }
            keepSelected -= Time.deltaTime;
        }
        else if (materialIndex == 1)
        {
            materialIndex = 0;
            renderer.sharedMaterial = materials[materialIndex];
        }
    }

    abstract public void onPress();

}
