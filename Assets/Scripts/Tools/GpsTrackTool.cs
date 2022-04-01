using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using System.IO;
using System;

public class GpsTrackTool : Tool
{

    public static bool shouldStopStoring;
    public override IEnumerator ToolCoroutine()
    {

       
        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();

       

        toolControllerComponent.GpsTrackControlUI.SetActive(true);
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            toolControllerComponent.OculusMasterObject.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.2783019f, 0.3061422f, 1, 1);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsTrackControlUI").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsTrackControlUI").transform.localPosition = new Vector3(0.0f, -530.0f, 0.0f);

        }

        PauseAndGUIBehaviour.isToolMenu = false;
        ToolController.ToolIsCurrentlyRunning = true;
        shouldStopStoring = true;

     

        yield return wfeof;

    }
    public void StopGPSTracking(bool notify=true)
    {
        
        toolControllerComponent.GpsStopControlButton.GetComponent<Button>().interactable = false;
        toolControllerComponent.GpsStartControlButton.GetComponent<Button>().interactable = true;

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsTrackControlUI").transform.Find("Content").transform.Find("StartGpsTrack").GetComponent<Button>().interactable = true;
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsTrackControlUI").transform.Find("Content").transform.Find("StopGpsTrack").GetComponent<Button>().interactable = false;
        }

       // if (notify)
            StartCoroutine(ShowNotification("Stopped", 1.5f));

        shouldStopStoring = true;
        
        
    }


    public void CancelButton()
    {
        // toolControllerComponent.ToolMenuInstance.GetComponent<CanvasGroup>().alpha = 1;
        if (!shouldStopStoring)
        {
            StartCoroutine(ShowNotification("Stop GPS Tracking before closing", 1.5f));
        }
        else
        {


           // StopGPSTracking(false);
            toolControllerComponent.GpsTrackControlUI.SetActive(false);

            if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            {
                GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsTrackControlUI").gameObject.SetActive(false);
            }


            if (PauseAndGUIBehaviour.isPause)
                PauseAndGUIBehaviour.isPause = false;
            ToolController.ToolIsCurrentlyRunning = false;
            shouldStopStoring = true;

        }


    }

    public void StartGPSTracking()
    {

        toolControllerComponent.GpsStopControlButton.GetComponent<Button>().interactable = true;
        toolControllerComponent.GpsStartControlButton.GetComponent<Button>().interactable = false;

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsTrackControlUI").transform.Find("Content").transform.Find("StartGpsTrack").GetComponent<Button>().interactable = false;
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsTrackControlUI").transform.Find("Content").transform.Find("StopGpsTrack").GetComponent<Button>().interactable = true;
           

        }
        shouldStopStoring = false;
        StartCoroutine(trackGPS());
        StartCoroutine(ShowNotification("Started", 1.5f));


    }

    IEnumerator trackGPS()
    {

       

        int counter = 001;

        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();

        var directory = new DirectoryInfo(Application.dataPath);
        var directoryPath = Path.Combine(directory.Parent.FullName, "Outputs/GpsTrack");
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
        }

        var path_csv = Path.Combine(directoryPath, string.Format("gpstrack_{0}", DateTime.Now.ToString("yyyyMMdd_Hmmssffff")));
        shouldStopStoring = false;

        //write geotag
        StreamWriter writer = new StreamWriter(path_csv + ".csv", true);
        writer.WriteLine("#id, Lat, Lon, z, UTC Time");
        writer.Close();
        /*
        toolControllerComponent.NotificationText.GetComponent<UnityEngine.UI.Text>().text = "Track Recording to " + path_csv;
        toolControllerComponent.NotificationPanel.SetActive(true);
        */

        while (!shouldStopStoring)
        {
          

            writer = new StreamWriter(path_csv + ".csv", true);
            //StreamWriter writer = new StreamWriter(path_csv + ".csv", true);
            writer.WriteLine(counter.ToString("000") + ", " + PositionSingleton.playerRealPosition.z.ToString("0.0000000000000", new CultureInfo("en-GB")) + ", " + PositionSingleton.playerRealPosition.x.ToString("0.0000000000000", new CultureInfo("en-GB")) + ", " + PositionSingleton.playerRealPosition.y.ToString("0.000", new CultureInfo("en-GB")) + ", " + DateTime.UtcNow);
            counter++;
            writer.Close();


            //yield return wfeof;
            yield return new WaitForSeconds(0.5f);

        }


        yield return null;


    }

    IEnumerator ShowNotification(string message, float delay)
    {
        GameObject.Find("Canvas").gameObject.transform.Find("GpsTrackControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = message;
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsTrackControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = message;
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsTrackControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().color =  new Color(1, 0, 0, 1); ;
        GameObject.Find("Canvas").gameObject.transform.Find("GpsTrackControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().color =  new Color(1, 0, 0, 1); ;
        /*
                GameObject.Find("Canvas").gameObject.transform.Find("Notification").Find("NotificationText").GetComponent<UnityEngine.UI.Text>().text = message;
                if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
                    GameObject.Find("Canvas_Oculus").gameObject.transform.Find("Notification").Find("NotificationText").GetComponent<UnityEngine.UI.Text>().text = message;

                //notificationText.GetComponent<UnityEngine.UI.Text>().enabled = true;
                GameObject.Find("Canvas").gameObject.transform.Find("Notification").gameObject.SetActive(true);
                if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
                    GameObject.Find("Canvas_Oculus").gameObject.transform.Find("Notification").gameObject.SetActive(true);
        */

        yield return new WaitForSeconds(delay);
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsTrackControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = "";
        GameObject.Find("Canvas").gameObject.transform.Find("GpsTrackControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = "";
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GpsTrackControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 1); ;
        GameObject.Find("Canvas").gameObject.transform.Find("GpsTrackControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 1); ;

        /*        //notificationText.GetComponent<UnityEngine.UI.Text>().enabled = false;
                GameObject.Find("Canvas").gameObject.transform.Find("Notification").gameObject.SetActive(false);
                if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
                    GameObject.Find("Canvas_Oculus").gameObject.transform.Find("Notification").gameObject.SetActive(false);
          */

    }




}


