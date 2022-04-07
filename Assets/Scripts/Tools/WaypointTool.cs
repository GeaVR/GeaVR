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
using System.Globalization;

public class WaypointTool : Tool
{
    int counter = 001;
    Sprite spriteImageInitial=null;

    public override IEnumerator ToolCoroutine()
    {
        if(!System.IO.File.Exists(toolControllerComponent.WaypointNameforSession))
        {
            StreamWriter writer = new StreamWriter(toolControllerComponent.WaypointNameforSession, true);
            writer.WriteLine("#id, Lat, Lon, z, name, note, picture");
            writer.Close();
        }
        
        if (spriteImageInitial == null)
            spriteImageInitial = toolControllerComponent.WaypointPictureImage.GetComponent<Image>().sprite;
        else
        {

            GameObject.Find("Canvas").gameObject.transform.Find("WaypointMenu").gameObject.transform.Find("Container").gameObject.transform.Find("Image_Preview").gameObject.transform.Find("Image").GetComponent<Image>().sprite = spriteImageInitial;
            if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            {
                GameObject.Find("Canvas_Oculus").gameObject.transform.Find("WaypointMenu").gameObject.transform.Find("Container").gameObject.transform.Find("Image_Preview").gameObject.transform.Find("Image").GetComponent<Image>().sprite = spriteImageInitial;

            }
        }

        //2D
        ToolController.ToolIsCurrentlyRunning = true;

        toolControllerComponent.WaypointIdCounter.GetComponent<Text>().text = "" + counter.ToString("000");
        toolControllerComponent.WaypointText.GetComponent<Text>().text = "WP_" + counter.ToString("000");
        toolControllerComponent.WaypointMenu.gameObject.transform.Find("Container").gameObject.transform.Find("Name").gameObject.transform.Find("InputField").GetComponent<InputField>().text = "wp_" + counter.ToString("000");
        toolControllerComponent.WaypointCorrdinatesText.GetComponent<Text>().text = "(" + PositionSingleton.playerRealPosition.z.ToString("0.000000", new CultureInfo("en-GB")) + ", " + PositionSingleton.playerRealPosition.x.ToString("0.000000", new CultureInfo("en-GB")) + " ," + PositionSingleton.playerRealPosition.y.ToString("0.000", new CultureInfo("en-GB")) + ")"; 
        toolControllerComponent.WaypointMenu.SetActive(true);

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            toolControllerComponent.OculusMasterObject.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.2783019f, 0.3061422f, 1, 1);

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("WaypointMenu").gameObject.transform.Find("UpperPanel").gameObject.transform.Find("id_counter_value").gameObject.GetComponent<Text>().text = "" + counter.ToString("000"); ;
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("WaypointMenu").gameObject.transform.Find("Container").gameObject.transform.Find("Name").gameObject.transform.Find("InputField").GetComponent<InputField>().text = "wp_" + counter.ToString("000");

            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("WaypointMenu").gameObject.transform.Find("Container").gameObject.transform.Find("Row_2_Layout").gameObject.transform.Find("NorthingEtc").gameObject.GetComponent<Text>().text = "(" + PositionSingleton.playerRealPosition.z.ToString("0.000000", new CultureInfo("en-GB")) + ", " + PositionSingleton.playerRealPosition.x.ToString("0.000000", new CultureInfo("en-GB")) + " ," + PositionSingleton.playerRealPosition.y.ToString("0.000", new CultureInfo("en-GB")) + ")";
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("WaypointMenu").transform.localPosition = new Vector3(0.0f, 150.0f, 0.0f);
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("WaypointMenu").gameObject.SetActive(true);
        }

        yield return new WaitForEndOfFrame();
    }
    
    public void CancelButton()
    {

        if (File.Exists(toolControllerComponent.WaypointPicture.GetComponent<Text>().text))
            File.Delete(toolControllerComponent.WaypointPicture.GetComponent<Text>().text);
        //2D
        ToolController.ToolIsCurrentlyRunning = false;
        toolControllerComponent.WaypointMenu.SetActive(false);

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("ToolMenu").GetComponent<CanvasGroup>().alpha =1;
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("WaypointMenu").Find("Container").Find("Notes").Find("InputField").GetComponent<InputField>().text = "";
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("WaypointMenu").gameObject.SetActive(false);
        }
        PauseAndGUIBehaviour.isToolMenu = false;
    }

    public void SaveButton()
    {
        //2D
        toolControllerComponent.WaypointMenu.SetActive(false);
        toolControllerComponent.WaypointNote.GetComponent<InputField>().text = "";

        String name = toolControllerComponent.WaypointText.GetComponent<Text>().text;
        String notes = toolControllerComponent.WaypointNote.GetComponent<InputField>().text;

        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("WaypointMenu").gameObject.SetActive(false);
            InputField oculusNotes = GameObject.Find("Canvas_Oculus").gameObject.transform.Find("WaypointMenu").Find("Container").Find("Notes").Find("InputField").GetComponent<InputField>();
            //if the 2D notes field has been used, then only the text in that one will be saved and the oculus one will be reset. Otherwise the oculus one is used.
            if (notes == "")
            {
                notes = oculusNotes.text;
            }
            oculusNotes.text = "";
        }

        //write geotag to file
        StreamWriter writer = new StreamWriter(toolControllerComponent.WaypointNameforSession, true);

        writer.WriteLine(counter.ToString("000") + ", " + PositionSingleton.playerRealPosition.z.ToString("0.0000000000000", new CultureInfo("en-GB")) + ", " + PositionSingleton.playerRealPosition.x.ToString("0.0000000000000", new CultureInfo("en-GB")) + ", " + PositionSingleton.playerRealPosition.y.ToString("0.000", new CultureInfo("en-GB")) + ", " + name + ", " + notes + ", " + toolControllerComponent.WaypointPicture.GetComponent<Text>().text);
        counter++;
        writer.Close();
        notes = "";
        toolControllerComponent.WaypointPicture.GetComponent<Text>().text = "";

        ToolController.ToolIsCurrentlyRunning = false;
        PauseAndGUIBehaviour.isToolMenu = false;
    }

}
