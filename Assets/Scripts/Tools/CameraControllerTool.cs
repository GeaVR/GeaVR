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
using System.IO;
using System;
using UnityEngine.EventSystems;
using UnityStandardAssets.Characters.FirstPerson;
using System.Globalization;
using UnityEditor;

public class CameraControllerTool : Tool
{
  //  public GameObject[] hands;
    bool isPlayVideo;
    static Vector3 oldpositionL;
    static Vector3 oldpositionR;
    int width = Screen.width;
    int height = Screen.height;

   

    public override IEnumerator ToolCoroutine()
    {

       
        if (!System.IO.File.Exists(toolControllerComponent.PhotoDatabaseFile))
        {
            StreamWriter writer = new StreamWriter(toolControllerComponent.PhotoDatabaseFile, true);
            writer.WriteLine("#id, path, photo_filename, metadata_filename, Lat, Lon, z, note");
            writer.Close();
        }
        else if (toolControllerComponent.ListPhoto.Count==0)
        {
            //create struct for photo
             

            StreamReader inp_stm = new StreamReader(toolControllerComponent.PhotoDatabaseFile);

            while (!inp_stm.EndOfStream)
            {
                string inp_ln = inp_stm.ReadLine();
                if (inp_ln.Substring(0,1).CompareTo("#")!=0 )
                {
                    // Do Something with the input. 
                    Debug.Log(inp_ln);
                    string[] parts = inp_ln.Split(char.Parse(","));
                    string note = "";
                    if (parts.Length == 6)
                        note = parts[7];

                    PhotoEntry photo = new PhotoEntry
                    {
                        Id = parts[0],
                        Path = parts[1],
                        PhotoName = parts[2],
                        MetadataName = parts[3],
                        Lat = parts[4],
                        Lon = parts[5],
                        Z = parts[6],
                        Note = note
                    };

                    toolControllerComponent.ListPhoto.Add(photo);

                }
            }

            inp_stm.Close();
        }

        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            toolControllerComponent.OculusMasterObject.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.2783019f, 0.3061422f, 1, 1);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("CameraControlUI").gameObject.SetActive(true);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("CameraControlUI").transform.localPosition = new Vector3(0.0f, -530.0f, 0.0f);
        }
        toolControllerComponent.CameraControlUI.SetActive(true);

        PauseAndGUIBehaviour.isToolMenu = false;

        ToolController.ToolIsCurrentlyRunning = true;


        yield return wfeof;

    }

    public void CancelButton()
    {
        // toolControllerComponent.ToolMenuInstance.GetComponent<CanvasGroup>().alpha = 1;
        toolControllerComponent.CameraControlUI.SetActive(false);
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("CameraControlUI").gameObject.SetActive(false);

        if (PauseAndGUIBehaviour.isPause)
            PauseAndGUIBehaviour.isPause = false;

        ToolController.ToolIsCurrentlyRunning = false;

    }

     public void CancelButtonStopTool()
    {


        GameObject.Find("Canvas").gameObject.transform.Find("PhotoPreview").gameObject.SetActive(false);
        GameObject.Find("Canvas").gameObject.transform.Find("CameraControlUI").gameObject.SetActive(false);//.GetComponent<CanvasGroup>().alpha = 1;

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("PhotoPreview").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("CameraControlUI").gameObject.SetActive(false);//.GetComponent<CanvasGroup>().alpha = 1;
        }



        if (PauseAndGUIBehaviour.isPause)
            PauseAndGUIBehaviour.isPause = false;

        ToolController.ToolIsCurrentlyRunning = false;

    }

    public void CancelButtonPhotoView()
    {
        GameObject.Find("Canvas").gameObject.transform.Find("PhotoPreview").gameObject.SetActive(false);
        GameObject.Find("Canvas").gameObject.transform.Find("CameraControlUI").gameObject.SetActive(true);//.GetComponent<CanvasGroup>().alpha = 1;

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("PhotoPreview").gameObject.SetActive(false);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("CameraControlUI").gameObject.SetActive(true);//.GetComponent<CanvasGroup>().alpha = 1;
        }

    }

    public void ViewNextPhoto()
    {

      
        if (toolControllerComponent.actualPhoto < toolControllerComponent.ListPhoto.Count - 1)
            toolControllerComponent.actualPhoto++;
        else if (toolControllerComponent.actualPhoto >= toolControllerComponent.ListPhoto.Count - 1)
            toolControllerComponent.actualPhoto = 0;

        PhotoEntry photo = toolControllerComponent.ListPhoto[toolControllerComponent.actualPhoto];
        ViewPhoto(photo);
    }

    public void ViewPreviousPhoto()
    {

        
        if (toolControllerComponent.actualPhoto > 0)
            toolControllerComponent.actualPhoto--;
        else if (toolControllerComponent.actualPhoto <= 0)
            toolControllerComponent.actualPhoto = toolControllerComponent.ListPhoto.Count - 1;

      
        PhotoEntry photo = toolControllerComponent.ListPhoto[toolControllerComponent.actualPhoto];
        ViewPhoto(photo);
    }

    public void ScreenCapture()
    {


    }

    public void TakePhoto()
    {

        toolControllerComponent.CameraControlUI.GetComponent<CanvasGroup>().alpha = 0;
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
             GameObject.Find("Canvas_Oculus").gameObject.transform.Find("CameraControlUI").GetComponent<CanvasGroup>().alpha = 0;
            //GameObject[] hands;
            //hands = GameObject.FindGameObjectsWithTag("Hands");  
           


            oldpositionL= toolControllerComponent.hands[0].transform.position;
            oldpositionR= toolControllerComponent.hands[1].transform.position;

            
            foreach (GameObject hand in toolControllerComponent.hands)
            {
                hand.transform.position = hand.transform.position + hand.transform.TransformDirection((Vector3.forward + Vector3.up * 1000f) * 10000f);
            }

        }
        StartCoroutine(makeScreenshot());

    }

    public void TakePhotoForWaypoint()
    {

        toolControllerComponent.WaypointMenu.GetComponent<CanvasGroup>().alpha = 0;

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("WaypointMenu").GetComponent<CanvasGroup>().alpha = 0;

            oldpositionL = toolControllerComponent.hands[0].transform.position;
            oldpositionR = toolControllerComponent.hands[1].transform.position;


            foreach (GameObject hand in toolControllerComponent.hands)
            {
                hand.transform.position = hand.transform.position + hand.transform.TransformDirection((Vector3.forward + Vector3.up * 1000f) * 10000f);
            }
        }


        if (File.Exists(toolControllerComponent.WaypointPicture.GetComponent<Text>().text))
            File.Delete(toolControllerComponent.WaypointPicture.GetComponent<Text>().text);

        StartCoroutine(makeScreenshotForWaypoint());

    }

    IEnumerator makeScreenshotForWaypoint()
    {

        yield return new WaitForSeconds(0.1f);

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

        // For testing purposes, also write to a file in the project folder
        File.WriteAllBytes(path, bytes);

        toolControllerComponent.WaypointPicture.GetComponent<Text>().text = "" + path;

        Texture2D sampleTexture = new Texture2D(width, height);
        bool isLoaded = sampleTexture.LoadImage(bytes);
        if (isLoaded)
        {
            Sprite sprite = Sprite.Create(sampleTexture, new Rect(0, 0, sampleTexture.width, sampleTexture.height), new Vector2(sampleTexture.width / 2, sampleTexture.height / 2));

            toolControllerComponent.WaypointPictureImage.GetComponent<Image>().sprite = sprite;
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("WaypointMenu").gameObject.transform.Find("Container").gameObject.transform.Find("Image_Preview").gameObject.transform.Find("Image").GetComponent<Image>().sprite = sprite;


            if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            {
            }
        }

     
        toolControllerComponent.WaypointMenu.GetComponent<CanvasGroup>().alpha = 1;

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {    

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("WaypointMenu").GetComponent<CanvasGroup>().alpha = 1;
            toolControllerComponent.hands[0].transform.position = oldpositionL;
            toolControllerComponent.hands[1].transform.position = oldpositionR;
        }

        yield return null;
    }

    IEnumerator makeScreenshot()
    {
        yield return new WaitForSeconds(0.1f);
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

        String date = DateTime.Now.ToString("yyyy.MMddHHmmss");

        var path = Path.Combine(directoryPath, string.Format("Screenshot_{0}.png", date));

        // For testing purposes, also write to a file in the project folder
        File.WriteAllBytes(path, bytes);
        //write geotag
        StreamWriter writer = new StreamWriter(path + ".txt", true);


        writer.WriteLine("Lat: " + PositionSingleton.playerRealPosition.z.ToString("0.0000000000000"));
        writer.WriteLine("Lon: " + PositionSingleton.playerRealPosition.x.ToString("0.0000000000000"));
        writer.WriteLine("Alt: " + PositionSingleton.playerRealPosition.y.ToString("0.000"));

        writer.Close();

        
        StreamWriter writerDb = new StreamWriter(toolControllerComponent.PhotoDatabaseFile, true);
        //   writer.WriteLine("id, path, photo_filename, metadata_filename, Lat, Lon, z, note");

        writerDb.WriteLine(date+","+directoryPath+","+string.Format("Screenshot_{0}.png", date)+","+string.Format("Screenshot_{0}.png.txt", date)+","+PositionSingleton.playerRealPosition.z.ToString("0.0000000000000", new CultureInfo("en-GB")) +","+ PositionSingleton.playerRealPosition.x.ToString("0.0000000000000", new CultureInfo("en-GB")) +","+ PositionSingleton.playerRealPosition.y.ToString("0.000", new CultureInfo("en-GB"))+", ");
        //counter.ToString("000") + ", " + PositionSingleton.playerRealPosition.z.ToString("0.0000000000000", new CultureInfo("en-GB")) + ", " + PositionSingleton.playerRealPosition.x.ToString("0.0000000000000", new CultureInfo("en-GB")) + ", " + PositionSingleton.playerRealPosition.y.ToString("0.000", new CultureInfo("en-GB")) + ", " + name + ", " + notes + ", " + toolControllerComponent.WaypointPicture.GetComponent<Text>().text);

        PhotoEntry photo = new PhotoEntry
        {
            Id = date,
            Path = directoryPath,
            PhotoName = string.Format("Screenshot_{0}.png", date),
            MetadataName = string.Format("Screenshot_{0}.png.txt", date),
            Lat = PositionSingleton.playerRealPosition.z.ToString("0.0000000000000", new CultureInfo("en-GB")),
            Lon = PositionSingleton.playerRealPosition.x.ToString("0.0000000000000", new CultureInfo("en-GB")),
            Z = PositionSingleton.playerRealPosition.y.ToString("0.000", new CultureInfo("en-GB")),
            Note = ""
             
        };

   
        writerDb.Close();


        toolControllerComponent.ListPhoto.Add(photo);


        PauseAndGUIBehaviour.isScreenshot = false;
        PauseAndGUIBehaviour.isToolMenu = false;
        PauseAndGUIBehaviour.isPause = false;

       

        toolControllerComponent.CameraControlUI.GetComponent<CanvasGroup>().alpha = 1;
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("CameraControlUI").GetComponent<CanvasGroup>().alpha = 1;

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {


            toolControllerComponent.hands[0].transform.position = oldpositionL;
            toolControllerComponent.hands[1].transform.position = oldpositionR;
        }

        StartCoroutine(ShowNotification("Done!", 1.5f));

        yield return null;


    }

  
    IEnumerator ShowNotification(string message, float delay)
    {
        GameObject.Find("Canvas").gameObject.transform.Find("CameraControlUI").transform.Find("LowerPanel").transform.Find("Camera_Tool_text").GetComponent<Text>().text = message;
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("CameraControlUI").transform.Find("LowerPanel").transform.Find("Camera_Tool_text").GetComponent<Text>().text = message;
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("CameraControlUI").transform.Find("LowerPanel").transform.Find("Camera_Tool_text").GetComponent<Text>().color = new Color(1, 0, 0, 1); ;
        GameObject.Find("Canvas").gameObject.transform.Find("CameraControlUI").transform.Find("LowerPanel").transform.Find("Camera_Tool_text").GetComponent<Text>().color = new Color(1, 0, 0, 1); ;

        yield return new WaitForSeconds(delay);
        
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("CameraControlUI").transform.Find("LowerPanel").transform.Find("Camera_Tool_text").GetComponent<Text>().text = "";
        GameObject.Find("Canvas").gameObject.transform.Find("CameraControlUI").transform.Find("LowerPanel").transform.Find("Camera_Tool_text").GetComponent<Text>().text = "";
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("CameraControlUI").transform.Find("LowerPanel").transform.Find("Camera_Tool_text").GetComponent<Text>().color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 1); ;
        GameObject.Find("Canvas").gameObject.transform.Find("CameraControlUI").transform.Find("LowerPanel").transform.Find("Camera_Tool_text").GetComponent<Text>().color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 1); ;
    }

    public void SavePhotoNoteToFile()
    {
        toolControllerComponent.ListPhoto[toolControllerComponent.actualPhoto].Note = GameObject.Find("Canvas_Oculus").gameObject.transform.Find("PhotoPreview").gameObject.transform.Find("PhotoNoteText").gameObject.transform.Find("Text").GetComponent<Text>().text ;
        
        GameObject.Find("Canvas").gameObject.transform.Find("PhotoPreview").gameObject.transform.Find("PhotoNoteTextVisible").GetComponent<Text>().text = toolControllerComponent.ListPhoto[toolControllerComponent.actualPhoto].Note;
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("PhotoPreview").gameObject.transform.Find("PhotoNoteTextVisible").GetComponent<Text>().text = toolControllerComponent.ListPhoto[toolControllerComponent.actualPhoto].Note;

        UpdateCSVFile();
    }

    public void ViewPhoto(PhotoEntry lastPhoto)
    {
        Texture2D sampleTexture = LoadPNG(lastPhoto.Path + "/" + lastPhoto.PhotoName, width, height);

        if (sampleTexture)
        {
            Sprite sprite = Sprite.Create(sampleTexture, new Rect(0, 0, sampleTexture.width, sampleTexture.height), new Vector2(sampleTexture.width / 2, sampleTexture.height / 2));

            toolControllerComponent.WaypointPictureImage.GetComponent<Image>().sprite = sprite;
            GameObject.Find("Canvas").gameObject.transform.Find("PhotoPreview").gameObject.transform.Find("Image").GetComponent<Image>().sprite = sprite;

            GameObject.Find("Canvas").gameObject.transform.Find("PhotoPreview").gameObject.transform.Find("PhotoNumberText").GetComponent<Text>().text = (toolControllerComponent.actualPhoto + 1) + "/"+ toolControllerComponent.ListPhoto.Count;

            GameObject.Find("Canvas").gameObject.transform.Find("PhotoPreview").gameObject.transform.Find("PhotoNameText").GetComponent<Text>().text = lastPhoto.PhotoName;
            GameObject.Find("Canvas").gameObject.transform.Find("PhotoPreview").gameObject.transform.Find("PhotoNoteTextVisible").GetComponent<Text>().text = lastPhoto.Note;


            if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            {
                GameObject.Find("Canvas_Oculus").gameObject.transform.Find("PhotoPreview").gameObject.transform.Find("Image").GetComponent<Image>().sprite = sprite;
                GameObject.Find("Canvas_Oculus").gameObject.transform.Find("PhotoPreview").gameObject.transform.Find("PhotoNumberText").GetComponent<Text>().text = (toolControllerComponent.actualPhoto +1) + "/" + toolControllerComponent.ListPhoto.Count;
                GameObject.Find("Canvas_Oculus").gameObject.transform.Find("PhotoPreview").gameObject.transform.Find("PhotoNameText").GetComponent<Text>().text = lastPhoto.PhotoName;
                GameObject.Find("Canvas_Oculus").gameObject.transform.Find("PhotoPreview").gameObject.transform.Find("PhotoNoteTextVisible").GetComponent<Text>().text = lastPhoto.Note;
            }
        }
    }

    public void ActivateViewer()
    {
        
        if (toolControllerComponent.ListPhoto.Count > 0)
        {

            GameObject.Find("Canvas").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.AddListener(SavePhotoNoteToFile);

            toolControllerComponent.actualPhoto = toolControllerComponent.ListPhoto.Count - 1;
           

            GameObject.Find("Canvas").gameObject.transform.Find("PhotoPreview").gameObject.SetActive(true);
            GameObject.Find("Canvas").gameObject.transform.Find("CameraControlUI").gameObject.SetActive(false);//.GetComponent<CanvasGroup>().alpha = 1;

            if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            {

                GameObject.Find("Canvas_Oculus").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("Accept").gameObject.GetComponent<Button>().onClick.AddListener(SavePhotoNoteToFile);

                GameObject.Find("Canvas_Oculus").gameObject.transform.Find("PhotoPreview").gameObject.SetActive(true);
                GameObject.Find("Canvas_Oculus").gameObject.transform.Find("CameraControlUI").gameObject.SetActive(false);//.GetComponent<CanvasGroup>().alpha = 1;
            }

       
                PhotoEntry lastPhoto = toolControllerComponent.ListPhoto[toolControllerComponent.actualPhoto];
                ViewPhoto(lastPhoto);
        }
        else
        {
            StartCoroutine(ShowNotification("No pictures", 1.5f));
        }
    }

    public void UpdateCSVFile()
    {
        var directory = new DirectoryInfo(Application.dataPath);
        var directoryPath = Path.Combine(directory.Parent.FullName, "Outputs/Camera/");

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (System.IO.File.Exists(toolControllerComponent.PhotoDatabaseFile))
            {
                StreamWriter writer = new StreamWriter(toolControllerComponent.PhotoDatabaseFile, false);
                writer.WriteLine("#id, path, photo_filename, metadata_filename, Lat, Lon, z");
                foreach (PhotoEntry p in toolControllerComponent.ListPhoto)
                {
                    writer.WriteLine(p.Id + "," + p.Path + "," + p.PhotoName + "," + p.MetadataName + "," + p.Lat + "," + p.Lon + "," + p.Z+","+p.Note);
                }
                writer.Close();
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public void DeleteSinglePhoto()
    {

        PhotoEntry lastPhoto = toolControllerComponent.ListPhoto[toolControllerComponent.actualPhoto];


        File.Delete(lastPhoto.Path+"/"+ lastPhoto.PhotoName);
        File.Delete(lastPhoto.Path+"/"+ lastPhoto.MetadataName);

        toolControllerComponent.ListPhoto.RemoveAt(toolControllerComponent.actualPhoto);

        UpdateCSVFile();

        if (toolControllerComponent.ListPhoto.Count == 0)
            CancelButtonPhotoView();
        else if (toolControllerComponent.actualPhoto > 0)
            ViewPreviousPhoto();
        else if (toolControllerComponent.actualPhoto == 0)
            ViewNextPhoto();
    }

    public void ClearAll()
    {
        Debug.Log("delete all");
        toolControllerComponent.ListPhoto.Clear();

        var directory = new DirectoryInfo(Application.dataPath);

        var directoryPath = Path.Combine(directory.Parent.FullName, "Outputs/Camera/");
        Directory.Delete(directoryPath);

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!System.IO.File.Exists(toolControllerComponent.PhotoDatabaseFile))
            {
                StreamWriter writer = new StreamWriter(toolControllerComponent.PhotoDatabaseFile, true);
                writer.WriteLine("#id, path, photo_filename, metadata_filename, Lat, Lon, z");
                writer.Close();
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
        }

        StartCoroutine(ShowNotification("Done!", 1.5f));
    }



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