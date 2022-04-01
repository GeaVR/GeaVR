using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRaycast : MonoBehaviour {

    private int m_RayLength = 100;

    public GameObject handController;
    public OVRInput.RawButton controller_button;
    public OVRInput.RawButton selection_text_button;
    
    private float waitTime = 0.2f;
    private float timer = 0.0f;
    private Abstract_VR_button lastCharPressed = null;


    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position, transform.forward, out hit, m_RayLength))
        {
            if (hit.collider.gameObject.GetComponent<Abstract_VR_button>())
            {
                //this is to highlight the pointed button
                hit.collider.gameObject.GetComponent<Abstract_VR_button>().keepSelected = .05f;

                if (OVRInput.GetDown(controller_button))
                {
                    hit.collider.gameObject.GetComponent<Abstract_VR_button>().onPress();
                   
                }
            }
            else if (hit.collider.gameObject.GetComponent<VRKeyboard_Text>() && OVRInput.GetUp(controller_button))
            {
                hit.collider.gameObject.GetComponent<VRKeyboard_Text>().OnSelect();
                hit.collider.gameObject.GetComponent<VRKeyboard_Text>().GoOnLastChar();
            }
            else if (hit.collider.gameObject.GetComponent<VRKeyboard_Text>() && OVRInput.GetUp(selection_text_button))
            {
                hit.collider.gameObject.GetComponent<VRKeyboard_Text>().OnSelect();
            }
        }

    }
}
