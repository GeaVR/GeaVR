using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityStandardAssets.Characters.FirstPerson;
[RequireComponent(typeof(FirstPersonController))]

public class MovementOculusTouchRegulator : MonoBehaviour {
    // Use this for initialization
    private FirstPersonController fpc;
    public GameObject RightOculusTouch;
    void Start () {
        fpc = GetComponent<FirstPersonController>();
	}
	
	// Update is called once per frame
	void Update () {
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS) {
            Vector2 move2d = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            Vector3 desiredMove = Vector3.forward * move2d.y + Vector3.right * move2d.x;
            fpc.desiredMove = RightOculusTouch.transform.TransformDirection(desiredMove);
        }
	}
}
