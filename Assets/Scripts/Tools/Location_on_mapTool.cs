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


public class Location_on_mapTool : Tool
{
    private Slider slider;

    public override IEnumerator ToolCoroutine()
    {
        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();
        toolControllerComponent.OculusMasterObject.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.2783019f, 0.3061422f, 1, 1);

        ToolController.ToolIsCurrentlyRunning = true;
        toolControllerComponent.ToolMenuInstance.GetComponent<CanvasGroup>().alpha = 0;
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsInfoOnMap").gameObject.SetActive(true);
            toolControllerComponent.OculusMasterObject.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.2783019f, 0.3061422f, 1, 1);
        }
            /*
        if (toolControllerComponent.OculusCanvas)
        {
            toolControllerComponent.OculusCanvas.transform.Find("GpsInfoOnMap").gameObject.active = true;
        }
        */
            /*
                    if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
                    {
                        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("ToolMenu").GetComponent<CanvasGroup>().alpha = 0;
                        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsInfoOnMap").gameObject.SetActive(true);
                    }

                    //2D
                    toolControllerComponent.LocationOnMapTool.SetActive(true);
                    toolControllerComponent.ToolMenuInstance.GetComponent<CanvasGroup>().alpha = 0;



                    //toolControllerComponent.ToolIsCurrentlyRunning = false;
                    ToolController.ToolIsCurrentlyRunning = false;

                */

            yield return wfeof;

    }

    public void CancelButton()
    {

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("ToolMenu").GetComponent<CanvasGroup>().alpha = 1;
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsInfoOnMap").gameObject.SetActive(false);
        }

        //2D
        toolControllerComponent.ToolMenuInstance.GetComponent<CanvasGroup>().alpha = 1;
        toolControllerComponent.LocationOnMapTool.SetActive(false);
        PauseAndGUIBehaviour.isToolMenu = false;
        ToolController.ToolIsCurrentlyRunning = false;
    }

    public void ZoomIn()
    {
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            slider = GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsInfoOnMap").gameObject.transform.Find("Slider").GetComponent<Slider>();
           
            slider.value -= 0.1f;
        }
    }
    public void ZoomOut()
    {
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            slider = GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsInfoOnMap").gameObject.transform.Find("Slider").GetComponent<Slider>();
           
            slider.value += 0.1f;
        }
    }
}
