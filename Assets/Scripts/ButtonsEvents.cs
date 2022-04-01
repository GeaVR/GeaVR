using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ButtonsEvents : MonoBehaviour {
    public StateSingleton.StateMode sm;
    public GameObject FatherController;
    void Start ()
    {
        GetComponent<Button>().onClick.AddListener(ChangeState);
    }

    void ChangeState()
    {
        StateSingleton.stateMode = sm;
        PauseAndGUIBehaviour.isModeMenu = false;
        //FatherController.GetComponent<MouseFreezeGame>().Pause(false);
    }
}
