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
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VRKeyboard_Text : MonoBehaviour {
    [HideInInspector] public bool selectionActive = false;
    [HideInInspector] public int charsPerRow = 28;
    [HideInInspector] public int position = 0;
    [HideInInspector] public int columnPosition;

    private KeyboardManager vrKeyboard = null;
    private InputField inputField = null;

    private float waitTime = 0.2f;
    private float timer = 0.0f;
    private OVRInput.RawButton lastPressed;

    private float caretShift = 1.0f;
    private bool isShifted = false;

    private void Start()
    {
        //it must never be 0, since it represents the max length of rows in the input field
        if (charsPerRow == 0)
        {
            charsPerRow = 28;
        }

        inputField = GetComponent<InputField>();
        vrKeyboard = GameObject.FindGameObjectWithTag("VRKeyboard").GetComponent<KeyboardManager>();
    }

    public void Update()
    {
        //Debug.Log("Caret Shift: " + caretShift);
        position = inputField.caretPosition;
        if (inputField.isFocused && OVRInput.GetUp(OVRInput.RawButton.RThumbstick))
        {
            selectionActive = !selectionActive;
        }
        else if (inputField.isFocused && OVRInput.Get(OVRInput.RawButton.RThumbstickLeft))
        {
            if (!isShifted)
            {
                StartCoroutine("accelerateCaret");
                isShifted = true;
            }
            if (lastPressed.Equals(OVRInput.RawButton.RThumbstickLeft))
            {
                timer += Time.deltaTime;
                if (timer > waitTime)
                {
                    timer = 0.0f;
                    if (selectionActive && inputField.selectionFocusPosition > 0)
                    {
                        inputField.selectionFocusPosition -= (int) caretShift;
                    }
                    else if (inputField.caretPosition > 0)
                    {
                        inputField.caretPosition -= (int) caretShift;
                    }
                    inputField.ForceLabelUpdate();
                }
            }
            else
            {
                timer = 0.0f;
                lastPressed = OVRInput.RawButton.RThumbstickLeft;
            }

        }
        else if (inputField.isFocused && OVRInput.Get(OVRInput.RawButton.RThumbstickRight))
        {
            if (!isShifted)
            {
                StartCoroutine("accelerateCaret");
                isShifted = true;
            }
            if (lastPressed.Equals(OVRInput.RawButton.RThumbstickRight))
            {
                timer += Time.deltaTime;
                if (timer > waitTime)
                {
                    timer = 0.0f;
                    if (selectionActive && inputField.selectionFocusPosition < inputField.text.Length)
                    {
                        inputField.selectionFocusPosition += (int) caretShift;
                    }
                    else if (inputField.caretPosition < inputField.text.Length)
                    {
                        inputField.caretPosition += (int) caretShift;
                    }
                    inputField.ForceLabelUpdate();
                }
            }
            else
            {
                timer = 0.0f;
                lastPressed = OVRInput.RawButton.RThumbstickRight;
            }
        }
    }

    private IEnumerator accelerateCaret()
    {
        while (OVRInput.Get(OVRInput.RawButton.RThumbstickRight) || OVRInput.Get(OVRInput.RawButton.RThumbstickLeft))
        {
            caretShift *= 1.1f;
            yield return new WaitForSeconds(.1f);
        }
        caretShift = 1.0f;
        isShifted = false;
    }

    private int previousRowIndex(string text, int caretPosition)
    {
        return caretPosition;
    }

    public void OnDeselect()
    {
        vrKeyboard.disable();

        inputField.DeactivateInputField();
    }

    public void OnSelect () {
        inputField = GetComponent<InputField>();
        if (vrKeyboard == null)
            vrKeyboard = GameObject.FindGameObjectWithTag("VRKeyboard").GetComponent<KeyboardManager>();

        vrKeyboard.enable(this);
        
        inputField.Select();
        inputField.ActivateInputField();
    }

    public void resetText()
    {
        inputField.text = "";
        inputField.ForceLabelUpdate();
    }

    public void setText(string text)
    {
        this.inputField.text = text;
    }

    public string getText()
    {
        return inputField.text;
    }

    public void GoOnLastChar()
    {
        StartCoroutine("SelectInputField");
    }

    IEnumerator SelectInputField()
    {
        yield return new WaitForEndOfFrame();
        inputField.caretPosition = inputField.text.Length;
    }

    public InputField GetInputField()
    {
        return inputField;
    }
}
