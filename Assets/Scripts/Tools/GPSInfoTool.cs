using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.IO;
using System;
using UnityEngine.EventSystems;

using UnityStandardAssets.Characters.FirstPerson;


public class GPSInfoTool : Tool
{
   
    public override IEnumerator ToolCoroutine()
    {
        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();

        GameObject.Find("Canvas").gameObject.transform.Find("GPSControlUI").gameObject.SetActive(true);

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            toolControllerComponent.OculusMasterObject.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.2783019f, 0.3061422f, 1, 1);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GPSControlUI").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GPSControlUI").transform.localPosition = new Vector3(0.0f, -530.0f, 0.0f);

        }
        // toolControllerComponent.CameraControlUI.SetActive(true);

        PauseAndGUIBehaviour.isToolMenu = false;
        ToolController.ToolIsCurrentlyRunning = true;


        yield return wfeof;

    }

   
    public void CancelButton()
    {
        // toolControllerComponent.ToolMenuInstance.GetComponent<CanvasGroup>().alpha = 1;
        GameObject.Find("Canvas").gameObject.transform.Find("GPSControlUI").gameObject.SetActive(false);

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GPSControlUI").gameObject.SetActive(false);



        ToolController.ToolIsCurrentlyRunning = false;

    }

   

    IEnumerator ShowNotification(string message, float delay)
    {

        toolControllerComponent.NotificationText.GetComponent<UnityEngine.UI.Text>().text = message;
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("Notification").Find("NotificationText").GetComponent<UnityEngine.UI.Text>().text = message;

        //notificationText.GetComponent<UnityEngine.UI.Text>().enabled = true;
        toolControllerComponent.NotificationPanel.SetActive(true);
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("Notification").gameObject.SetActive(true);



        yield return new WaitForSeconds(delay);
        //notificationText.GetComponent<UnityEngine.UI.Text>().enabled = false;
        toolControllerComponent.NotificationPanel.SetActive(false);
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("Notification").gameObject.SetActive(false);

    }



}
