using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SharpKml;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System.Globalization;
using UnityEngine.EventSystems;

public class Tool : MonoBehaviour
{    
    public enum toolType
    {
        PLACEMARK,
        POLYGON,
        LINE,
        PROFILE,
        RULER,
        SURFACE,
        OTHER
    }
    public GameObject toolController;
 	public bool isImporter = false;

    // Common Gameobjects required by tools    
    protected Transform master, directionMaster;
    protected ToolController toolControllerComponent;
    protected VirtualMeter VirtualCoord;
    UnityEvent OculusTouchTrigger, OculusTouchTriggerOn;
    
	private static bool isHolding;
    
    void Start()
    {
        toolControllerComponent = toolController.GetComponent<ToolController>();
        VirtualCoord = toolControllerComponent.VirtualMeterGameObject.GetComponent<VirtualMeter>();
    }

    // public interface 
    public void OnUse( )
    {
        
        toolControllerComponent.OculusMasterObject.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);

        master          = toolControllerComponent.GetMaster();
        directionMaster = toolControllerComponent.GetDirectionMaster();

        if ( !ToolController.ToolIsCurrentlyRunning ) 
        {
            PauseAndGUIBehaviour.isPause = false;
            PauseAndGUIBehaviour.isToolMenu = false;
       	    if (isImporter)
            {
                //string path = EditorUtility.OpenFilePanel("Open CSV", "", "csv"); // disabled for now, need to find a non-editor file panel
                string path = "";
                if (path.Length != 0)
                {
                    LoadFromFile(path);
                }
            }
            else
            {
                ToolController.ToolIsCurrentlyRunning = true;
                toolControllerComponent.StartCoroutine(ToolCoroutine()); // run coroutines on tool controller
            }
        }
    }         
   
    public virtual IEnumerator ToolCoroutine( )
    {
        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();
        ToolController.ToolIsCurrentlyRunning = false;
        yield return wfeof;
    }

    public static bool checkIfToolShouldQuit()
    {
        if (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) <= 0.3f && isHolding)
        {
           
            isHolding = false;
        }

        if (Input.GetMouseButton(1) 
            //|| OVRInput.GetDown(OVRInput.Button.One) 
            //|| OVRInput.GetDown(OVRInput.Button.Two)  
            //|| OVRInput.GetDown(OVRInput.Button.Three) 
            //|| OVRInput.GetDown(OVRInput.Button.Four)
            || OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.3f && !isHolding
            //|| PauseAndGUIBehaviour.isPause == true
            //|| PauseAndGUIBehaviour.isModeMenu == true
            //|| PauseAndGUIBehaviour.isToolMenu == true            
    )
        {

            
            isHolding = true;
            return true;
        }
        return false;
    }
    
    // import / export

    public virtual GameObject LoadFromFile(string FilePath)
    {
        return null;
    }

    public static void SaveSingleInstance(ToolInstance instance)
    {
        return;
    }

    public static void SaveMultiInstance()
    {
        return;
    }

    public static void DeleteAllInstances()
    {
        return;

    }

    public static IEnumerator ShowNotification(string message, float delay)
    {
        GameObject.Find("Canvas").gameObject.transform.Find("Notification").Find("NotificationText").GetComponent<UnityEngine.UI.Text>().text = message;
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("Notification").Find("NotificationText").GetComponent<UnityEngine.UI.Text>().text = message;

        //notificationText.GetComponent<UnityEngine.UI.Text>().enabled = true;
        GameObject.Find("Canvas").gameObject.transform.Find("Notification").gameObject.SetActive(true);
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("Notification").gameObject.SetActive(true);
        
        yield return new WaitForSeconds(delay);
        //notificationText.GetComponent<UnityEngine.UI.Text>().enabled = false;
        GameObject.Find("Canvas").gameObject.transform.Find("Notification").gameObject.SetActive(false);
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("Notification").gameObject.SetActive(false);
    }

    public static IEnumerator ShowNotificationLabelForMesuring(string message, float delay)
    {
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().color = new Color(1, 0, 0, 1); ;
        GameObject.Find("Canvas").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().color = new Color(1, 0, 0, 1); ;

        GameObject.Find("Canvas").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = message;
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = message;
        

        yield return new WaitForSeconds(delay);
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = "";
        GameObject.Find("Canvas").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().text = "";
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 1); ;
        GameObject.Find("Canvas").gameObject.transform.Find("MeasurementControlUI").transform.Find("LowerPanel").transform.Find("GpsTrack_Tool_text").GetComponent<Text>().color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 1); ;

    }

   
}
