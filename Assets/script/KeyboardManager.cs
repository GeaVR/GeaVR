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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class KeyboardManager : MonoBehaviour {
    private VRKeyboard_Text vrKeyboardText = null;
    private InputField inputField = null;
    public List<Char_VR_button> buttonsToSwitch = new List<Char_VR_button>();

    public DictationRecognizer dictationRecognizer;

    private void Start()
    {   
        this.disable();
        dictationRecognizer = new DictationRecognizer();
        dictationRecognizer.DictationResult += type_string;
       
    }

    private void setVRKeyboard_Text(VRKeyboard_Text vrKeyboardText)
    {
        this.vrKeyboardText = vrKeyboardText;
        if (vrKeyboardText != null)
            this.inputField = vrKeyboardText.GetInputField();
        else
            this.inputField = null;
    }

    public void type_char(char input)
    {
        int selectionBeginIndex = inputField.selectionAnchorPosition;
        int selectionEndIndex = inputField.selectionFocusPosition;
        if (inputField.selectionFocusPosition < inputField.selectionAnchorPosition)
        {
            selectionBeginIndex = inputField.selectionFocusPosition;
            selectionEndIndex = inputField.selectionAnchorPosition; 
        }

        //the input field is full and nothing is selected, so the char cannot be typed
        if (inputField.characterLimit > 0 && inputField.text.Length == inputField.characterLimit && selectionBeginIndex == selectionEndIndex)
        {
            return;
        }

        inputField.text = inputField.text.Substring(0, selectionBeginIndex) + input + inputField.text.Substring(selectionEndIndex, inputField.text.Length - selectionEndIndex);
        inputField.caretPosition = selectionBeginIndex + 1;
        vrKeyboardText.selectionActive = false;
        
    }

    public void type_string(string text, ConfidenceLevel confidence)
    {
        int selectionBeginIndex = inputField.selectionAnchorPosition;
        int selectionEndIndex = inputField.selectionFocusPosition;
        if (inputField.selectionFocusPosition < inputField.selectionAnchorPosition)
        {
            selectionBeginIndex = inputField.selectionFocusPosition;
            selectionEndIndex = inputField.selectionAnchorPosition;
        }

        //the input field is full and nothing is selected, so the char cannot be typed
        if (inputField.characterLimit > 0 && inputField.text.Length == inputField.characterLimit && selectionBeginIndex == selectionEndIndex)
        {
            return;
        }

        if (!vrKeyboardText.selectionActive)
            text += " ";

        inputField.text = inputField.text.Substring(0, selectionBeginIndex) + text + inputField.text.Substring(selectionEndIndex, inputField.text.Length - selectionEndIndex);
        inputField.caretPosition = selectionBeginIndex + text.Length;
        vrKeyboardText.selectionActive = false;
    }

    public void backspace ()
    {
        int selectionBeginIndex = inputField.selectionAnchorPosition;
        int selectionEndIndex = inputField.selectionFocusPosition;
        if (inputField.selectionFocusPosition < inputField.selectionAnchorPosition)
        {
            selectionBeginIndex = inputField.selectionFocusPosition;
            selectionEndIndex = inputField.selectionAnchorPosition;
        }
        if (selectionEndIndex > 0)
        {
            //no text is highlighted, so the char before the caretPosition is selected for being deleted right below this if
            if (selectionBeginIndex == selectionEndIndex)
            {
                selectionBeginIndex -= 1;
            }
            inputField.text = inputField.text.Substring(0, selectionBeginIndex) + inputField.text.Substring(selectionEndIndex, inputField.text.Length - selectionEndIndex);
            inputField.caretPosition = selectionBeginIndex;
        }
        vrKeyboardText.selectionActive = false;

        //if (inputField.text.Length > 0)
        //{
        //    //delete last char of the textBox
        //    inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
        //    inputField.caretPosition = inputField.text.Length;
        //}
    }
    
    public void enable(VRKeyboard_Text vrKeyboardText)
    {
        this.setVRKeyboard_Text(vrKeyboardText);
        this.toLowCharButtons();
        for (int i= 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    public void disable()
    {
        this.setVRKeyboard_Text(null);
        this.toLowCharButtons();
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void toLowCharButtons()
    {
        foreach (Char_VR_button button in buttonsToSwitch)
        {
            button.switchToLow();
        }
    }
}
