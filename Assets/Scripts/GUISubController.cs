using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUISubController : MonoBehaviour {
    public GameObject[] GuiToReset;

    // Use this for initialization
 //   void OnEnable () {
 //       foreach (GameObject go in GuiToReset)
 //           go.SetActive(false);
	//}

    private void OnDisable()
    {
        foreach (GameObject go in GuiToReset)
            go.SetActive(false);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
