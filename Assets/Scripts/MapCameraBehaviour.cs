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

public class MapCameraBehaviour : MonoBehaviour {
    public GameObject target;
    public GameObject targetPointer;
    public float HEIGHT = 0;
    float min;
    float max;
    float zoomVal;

    // Update is called once per frame
    void Start()
    {

        if (HEIGHT == 0) HEIGHT = transform.position.x> transform.position.z? transform.position.x: transform.position.z;

        min = HEIGHT * 4;
        max = HEIGHT;
        zoomVal = HEIGHT * 2;

      

    }
	void Update () {


        transform.position = target.transform.position + Vector3.up * zoomVal;
        
        targetPointer.transform.rotation = Quaternion.Euler(0, 0, -1*(target.transform.eulerAngles.y));

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {

            Vector3 rotationVector = new Vector3(0,(target.transform.eulerAngles.y ),-1* (target.transform.eulerAngles.y));

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsInfoOnMap").gameObject.transform.Find("GameObject").gameObject.transform.Find("GameObject").transform.rotation = Quaternion.Euler(rotationVector);
            //GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsInfoOnMap").gameObject.transform.Find("GameObject").gameObject.transform.Find("GameObject").transform.rotation = Quaternion.Euler(rotationVector_after);

        }

    }


    public void ZoomEdit()
    {
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {

            float zoomValue = GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsInfoOnMap").gameObject.transform.Find("Slider").GetComponent<Slider>().value;

      
             zoomVal = min *  zoomValue;


       
        } 
    }

  
}
