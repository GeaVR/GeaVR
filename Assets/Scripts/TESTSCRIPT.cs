using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTSCRIPT : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        foreach (var x in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown((KeyCode)x))
                print(x);
        }


    }
}
