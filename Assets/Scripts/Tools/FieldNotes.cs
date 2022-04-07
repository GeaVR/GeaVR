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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using  TMPro;

public class FieldNotes : Tool {

    [HideInInspector]
    public static String Notes = "";

    //if this is assigned to an InputField object in the scene, then the output of the note text is assigned to this object, instead of saving in the text file.
    public InputField outputField = null;

    public string outputString = null;
    public GameObject menuToHide = null;


    private VRKeyboard_Text vrKeyboard_InputField;
    private GameObject notesMenu;
    private int notesId = 1;
    private string date = null;


    public void setOutputField(InputField outputField)
    {
        this.outputField = outputField;
    }

    public void setOutputString(string outputString)
    {
        outputString = this.outputString;
    }

    public void setMenuToHide(GameObject menuToHide)
    {
        this.menuToHide = menuToHide;
    }

    public new void OnDirectUse()
    {
        outputString = null;
        OnUse();
    }

    public new void OnUse()
    {
        vrKeyboard_InputField = GameObject.Find("Canvas_Oculus").gameObject.transform.Find("NotesMenu").gameObject.transform.Find("Canvas").gameObject.transform.Find("Panel").gameObject.transform.Find("VRKeyboard_InputField").GetComponent<VRKeyboard_Text>();
        notesMenu = GameObject.Find("Canvas_Oculus").gameObject.transform.Find("NotesMenu").gameObject;

        if (date == null)
            date = DateTime.Now.ToString("yyyyMMdd_Hmm");

        if (!ToolController.ToolIsCurrentlyRunning)
        {
            PauseAndGUIBehaviour.isPause = true;
            ToolController.ToolIsCurrentlyRunning = true;
            PauseAndGUIBehaviour.isToolMenu = false;

        }
        toolControllerComponent.NotesMenu.gameObject.SetActive(true);
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            notesMenu.SetActive(true);
            vrKeyboard_InputField.OnSelect();
            if (outputField != null)
            {
                vrKeyboard_InputField.setText(this.outputField.text);
                if(menuToHide != null)
                    menuToHide.SetActive(false);
            }
            if (outputString != null)
            {
                vrKeyboard_InputField.setText(this.outputString);
                if (menuToHide != null)
                    menuToHide.SetActive(false);
            }
        }
    }
    
    public void OnClose()
    {
        toolControllerComponent.NotesMenu.gameObject.SetActive(false);
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
        {
            vrKeyboard_InputField.OnDeselect();
            vrKeyboard_InputField.resetText();
            notesMenu.SetActive(false);
            if (outputField != null || outputString != null)
            {
                if (menuToHide != null)
                {
                    menuToHide.SetActive(true);
                    outputField = null;
                    //outputString = null;
                    menuToHide = null;
                }
            }

        }

        PauseAndGUIBehaviour.isPause = false;
        ToolController.ToolIsCurrentlyRunning = false;
    }
    
    public void OnUpdateString(string _newNotes)
    {
        Notes = _newNotes;
    }


    public void sendTextToOutput()
    {
        Debug.Log(">sendTextToOutput");
        if (outputField == null & outputString == null  )
        {
            Debug.Log(">if");
            SaveNotesToTXT();
        }
        else if(outputField != null)
        {
            Debug.Log(">else if");
            outputField.text = vrKeyboard_InputField.getText();
        }
        else
        {
            Debug.Log(">else");
            outputString = vrKeyboard_InputField.getText();
           
        }
        this.OnClose();
    }

    public void SaveNotesToTXT()
    {       
        var directory = new DirectoryInfo(Application.dataPath);
        var directoryPath = Path.Combine(directory.Parent.FullName, "Outputs/Notes");

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

        var path = Path.Combine(directoryPath, string.Format("{0}.txt", ("Notes_" + date)));

        StreamWriter sr = null;
        if (!File.Exists(path))
        {
            sr = File.CreateText(path);
        }
        else
        {
            sr = File.AppendText(path);
        }
        sr.WriteLine("Note_" + notesId+" "+ DateTime.Now.ToString("yyyyMMdd_Hmmssffff"));
        sr.WriteLine(vrKeyboard_InputField.getText());
        sr.WriteLine();
        sr.Close();

        notesId += 1;
    }

    public IEnumerator MenuCoroutine()
    {
        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();

        bool menuShouldStayOpen = true;
        while (menuShouldStayOpen)
        {
            Cursor.visible = true; // todo - remove this once UI no longer ticks
            menuShouldStayOpen = !(checkIfToolShouldQuit());
            yield return wfeof;
        }
        toolControllerComponent.NotesMenu.gameObject.SetActive(false);
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS)
            GameObject.Find("Canvas_Oculus").gameObject.transform.Find("NotesMenu").gameObject.SetActive(false);

        PauseAndGUIBehaviour.isPause = false;
    }
}
