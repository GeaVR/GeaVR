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
