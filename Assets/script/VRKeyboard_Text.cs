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
        // TODO: this should be to move the cursor up and downrow in the text
        //else if (inputField.isFocused && OVRInput.Get(OVRInput.RawButton.RThumbstickUp))
        //{
        //    if (lastPressed.Equals(OVRInput.RawButton.RThumbstickUp))
        //    {
        //        timer += Time.deltaTime;
        //        if (timer > waitTime)
        //        {
        //            timer = 0.0f;
        //            if (selectionActive)
        //            {
        //                inputField.selectionFocusPosition = previousRowIndex(inputField.text, inputField.selectionFocusPosition); //System.Math.Max(0, inputField.selectionFocusPosition - 10);
        //            }
        //            else
        //            {
        //                inputField.caretPosition = previousRowIndex(inputField.text, inputField.caretPosition);
        //            }
        //            inputField.ForceLabelUpdate();
        //        }
        //    }
        //    else
        //    {
        //        timer = 0.0f;
        //        lastPressed = OVRInput.RawButton.RThumbstickUp;
        //    }
        //}
        //else if (inputField.isFocused && OVRInput.Get(OVRInput.RawButton.RThumbstickDown))
        //{
        //    if (lastPressed.Equals(OVRInput.RawButton.RThumbstickDown))
        //    {
        //        timer += Time.deltaTime;
        //        if (timer > waitTime)
        //        {
        //            timer = 0.0f;
        //            if (selectionActive)
        //            {
        //                inputField.selectionFocusPosition = System.Math.Min(inputField.text.Length, inputField.selectionFocusPosition + charsPerRow);
        //            }
        //            else
        //            {
        //                inputField.caretPosition = System.Math.Min(inputField.text.Length, inputField.caretPosition + charsPerRow);
        //            }
        //            inputField.ForceLabelUpdate();
        //        }
        //    }
        //    else
        //    {
        //        timer = 0.0f;
        //        lastPressed = OVRInput.RawButton.RThumbstickDown;
        //    }
        //}
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
        /*
        columnPosition = caretPosition;
        int rowIndex = 0;
        int actualCharShift = 0;

        string previousText = text.Substring(caretPosition - charsPerRow, charsPerRow);
        print(previousText);
                
        string[] rows = text.Split('\n');
        List<int> visualizedRowsLengths = new List<int>();
        bool found = false;

        foreach (string row in rows)
        {
            int nextLength = row.Length;
            while (nextLength >= charsPerRow)
            {
                visualizedRowsLengths.Add(charsPerRow);
                nextLength -= charsPerRow;
                if ((actualCharShift + charsPerRow) < caretPosition)
                {
                    actualCharShift += charsPerRow;
                    rowIndex += 1;
                }
                else
                {
                    found = true;
                    break;
                }
            }
            visualizedRowsLengths.Add(nextLength+1);    //+1 is for the \n char which was lost in 'text.Split('\n')'
            if (!found && (actualCharShift + nextLength + 1) < caretPosition)
            {
                actualCharShift += nextLength + 1;
                rowIndex += 1;
            }
            else
                break;
        }
        while(rowIndex < visualizedRowsLengths.Count && columnPosition > visualizedRowsLengths[rowIndex])
        {
            columnPosition -= (visualizedRowsLengths[rowIndex]);

            if (rowIndex > 0)   //actualCharShift describes the shift of caret up to the previous row of the current one
                actualCharShift += (visualizedRowsLengths[rowIndex-1]);
        }
        if (rowIndex == 0)
            return 0;
        else
            return actualCharShift + caretPosition - visualizedRowsLengths[visualizedRowsLengths.Count-1];
            */

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
