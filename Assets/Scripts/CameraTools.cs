/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class CameraTools : MonoBehaviour
{

    public static bool isActiveScreenshot = false;
    private bool isMeActiveScreenshot = true;
    public static bool isActiveScreenshotForWaypoint = false;
    private bool isMeActiveScreenshotForWaypoint = true;

    public static bool needToReactivateGPS = false;
    public GameObject GuiToHidePauseScreenshot;
    public GameObject notificationText;
    public GameObject notificationPanel;
    public GameObject imageForWaypoint;
    public GameObject hiddenPathForWaypoint = null;


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (isActiveScreenshot != isMeActiveScreenshot)
        {
            isMeActiveScreenshot = isActiveScreenshot;
        }

        if (isActiveScreenshotForWaypoint != isMeActiveScreenshotForWaypoint)
        {
            isMeActiveScreenshotForWaypoint = isActiveScreenshotForWaypoint;
        }

        if (isActiveScreenshot)
        {
            StartCoroutine(TakeScreenshot());
        }
        else
        {
            isActiveScreenshot = false;
        }

        if (isActiveScreenshotForWaypoint)
        {
            StartCoroutine(TakeScreenshotForWaypoint());
        }
        else
        {
            isActiveScreenshotForWaypoint = false;
        }

    }

    IEnumerator TakeScreenshotForWaypoint()
    {


            GuiToHidePauseScreenshot.SetActive(false);

        // We should only read the screen buffer after rendering is complete
        isActiveScreenshotForWaypoint = false;

        yield return new WaitForEndOfFrame();

        // Create a texture the size of the screen, RGB24 format
        int width = Screen.width;
        int height = Screen.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Read screen contents into the texture
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        Destroy(tex);

        var directory = new DirectoryInfo(Application.dataPath);

        var directoryPath = Path.Combine(directory.Parent.FullName, "Outputs/Waypoints");
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
        var path = Path.Combine(directoryPath, string.Format("Screenshot_{0}.png", DateTime.Now.ToString("yyyyMMdd_Hmmssffff")));
        File.WriteAllBytes(path, bytes);

        hiddenPathForWaypoint.GetComponent<Text>().text = "" + path;




        Texture2D sampleTexture = new Texture2D(width, height);
        bool isLoaded = sampleTexture.LoadImage(bytes);
        if (isLoaded)
        {
            Debug.Log("Loaded");
            Sprite sprite = Sprite.Create(sampleTexture, new Rect(0, 0, sampleTexture.width, sampleTexture.height), new Vector2(sampleTexture.width / 2, sampleTexture.height / 2));

            imageForWaypoint.GetComponent<Image>().sprite = sprite;
            //imageForWaypoint.GetComponent<RawImage>().texture = sampleTexture;
        }

        GuiToHidePauseScreenshot.SetActive(true);



    }


    IEnumerator TakeScreenshot()
    {
       

        // We should only read the screen buffer after rendering is complete
        isActiveScreenshot = false;

        yield return new WaitForEndOfFrame();

        // Create a texture the size of the screen, RGB24 format
        int width = Screen.width;
        int height = Screen.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Read screen contents into the texture
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        Destroy(tex);

        var directory = new DirectoryInfo(Application.dataPath);

        var directoryPath = Path.Combine(directory.Parent.FullName, "Outputs/Camera");
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
        var path = Path.Combine(directoryPath, string.Format("Screenshot_{0}.png", DateTime.Now.ToString("yyyyMMdd_Hmmssffff")));

        // For testing purposes, also write to a file in the project folder
        File.WriteAllBytes(path, bytes);
        //write geotag
        StreamWriter writer = new StreamWriter(path + ".txt", true);


        writer.WriteLine("Northing: " + PositionSingleton.playerRealPosition.z.ToString("0.000"));
        writer.WriteLine("Easting: " + PositionSingleton.playerRealPosition.x.ToString("0.000"));
        writer.WriteLine("Altitude: " + PositionSingleton.playerRealPosition.y.ToString("0.000"));

        StartCoroutine(ShowMessage("Taking screenshot to " + path, 2));

        PauseAndGUIBehaviour.isScreenshot = false;
        PauseAndGUIBehaviour.isToolMenu = false;

        PauseAndGUIBehaviour.isPause = false;

        writer.Close();



        print("Finished Uploading Screenshot");



        yield return null;


    }

    IEnumerator ShowMessage(string message, float delay)
    {

        notificationText.GetComponent<UnityEngine.UI.Text>().text = message;
        //notificationText.GetComponent<UnityEngine.UI.Text>().enabled = true;
        notificationPanel.SetActive(true);

        yield return new WaitForSeconds(delay);
        //notificationText.GetComponent<UnityEngine.UI.Text>().enabled = false;
        notificationPanel.SetActive(false);

    }


}

*/