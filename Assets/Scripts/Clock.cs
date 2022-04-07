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
        }
    }
}