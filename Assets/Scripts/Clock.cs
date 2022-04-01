using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

// © 2017 TheFlyingKeyboard and released under MIT License
// theflyingkeyboard.net

public class Clock : MonoBehaviour
{
    public Text clockText;
    public Text DateText;

    public bool showSeconds;

    private int seconds;
    private int minutes;
    private DateTime time;

    // Use this for initialization
    void Start()
    {
        seconds = -1;
        minutes = -1;

        DateText.text = System.DateTime.Now.ToString("yyyy/MM/dd");
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GPSControlUI").gameObject.transform.Find("Panel").gameObject.transform.Find("Date").GetComponent<Text>().text = System.DateTime.Now.ToString("yyyy/MM/dd");


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        time = DateTime.Now;

        if (showSeconds)
        {
            if (seconds != time.Second)
            {
                UpdateText();
                seconds = time.Second;
            }
        }
        else
        {
            if (minutes != time.Minute)
            {
                UpdateText();
                minutes = time.Minute;
            }
        }
    }

    void UpdateText()
    {
        clockText.text = time.Hour.ToString("D2") + ":" + time.Minute.ToString("D2");
        GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GPSControlUI").gameObject.transform.Find("Panel").gameObject.transform.Find("Clock").GetComponent<Text>().text = time.Hour.ToString("D2") + ":" + time.Minute.ToString("D2");


        if (showSeconds)
        {
            clockText.text += ":" + time.Second.ToString("D2");
        //    GameObject.Find("Canvas_Oculus").gameObject.transform.Find("GPSControlUI").gameObject.transform.Find("Panel").gameObject.transform.Find("Clock").GetComponent<Text>().text += ":" + time.Second.ToString("D2");

        }
    }
}