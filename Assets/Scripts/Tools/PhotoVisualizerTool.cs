using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharpKml;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System.IO;

/// <summary>
/// 
/// </summary>
public class PhotoVisualizerTool : Tool
{
    [HideInInspector]
    public static List<GameObject> instanceList = new List<GameObject>();

    public override IEnumerator ToolCoroutine()
    {
        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();
        ToolController.ToolIsCurrentlyRunning = true;

        Debug.Log(toolControllerComponent.PhotoDatabaseFile);

        yield return wfeof;
       
       
    }

   

    public void CancelButton()
    {

        Debug.Log("Cancello");
    }



/*
 * using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.IO;
using System;
using System.Globalization;

public class PhotoVisualizerTool : Tool
{
    int width = Screen.width;
    int height = Screen.height;

    public override IEnumerator ToolCoroutine()
    {
        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();
        PauseAndGUIBehaviour.isToolMenu = false;

        ToolController.ToolIsCurrentlyRunning = true;


        yield return wfeof;
    }



    public void startToolInterface()
    {


        Debug.Log(toolControllerComponent.PhotoDatabaseFile);
        

        GameObject.Find("Canvas").gameObject.transform.Find("PhotoPreview").gameObject.SetActive(true);

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("PhotoPreview").gameObject.SetActive(true);
        }

       
        PauseAndGUIBehaviour.isToolMenu = false;
        ToolController.ToolControllerInterfaceIsCurrentlyRunning = true;


        //  StartCoroutine(this.OnUse());
    }



    /*
    public void tetst()
    {

        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("PhotoPreview").gameObject.SetActive(true);

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("PhotoPreview").gameObject.SetActive(true);
        }

       

     
        PhotoEntry lastPhoto = toolControllerComponent.ListPhoto[0];

        Texture2D sampleTexture = LoadPNG(lastPhoto.Path+"/"+lastPhoto.PhotoName ,width, height);

        if (sampleTexture)
        {
            Sprite sprite = Sprite.Create(sampleTexture, new Rect(0, 0, sampleTexture.width, sampleTexture.height), new Vector2(sampleTexture.width / 2, sampleTexture.height / 2));

            toolControllerComponent.WaypointPictureImage.GetComponent<Image>().sprite = sprite;
            GameObject.Find("Canvas").gameObject.transform.Find("PhotoPreview").gameObject.transform.Find("Image").GetComponent<Image>().sprite = sprite;


            if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            {
                GameObject.Find("Canvas_Oculus").gameObject.transform.Find("PhotoPreview").gameObject.transform.Find("Image").GetComponent<Image>().sprite = sprite;
            }
            //imageForWaypoint.GetComponent<RawImage>().texture = sampleTexture;
        }
   
    }
       */

public Texture2D LoadPNG(string filePath, int width, int height)
{

    Texture2D tex = null;
    byte[] fileData;

    if (File.Exists(filePath))
    {
        fileData = File.ReadAllBytes(filePath);
        tex = new Texture2D(width, height);
        tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
    }
    return tex;
}

}

