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
using System.IO;
using UnityEngine;

public class TileData : MonoBehaviour {
    public string tileFileName;
    public Vector3Decimal tileRealPosition;
    public decimal globalScaleFactor;
    public decimal globalHeightScaleFactor;

    public TileObject tileObject;
    public Vector3 terrainSize;
    public Vector3 terrainPosition;
    public int totalNumberOfTiles;
    // Use this for initialization
    void Start()
    {
        //Initialize Tile Object
        string path = "Tiles/" + tileFileName;
        TextAsset ta = (TextAsset)Resources.Load(path);
        string json = ta.text;
        tileObject = new TileObject();
        tileObject = JsonUtility.FromJson<TileObject>(json);
        print(tileObject);
        decimal x = (decimal.Parse(tileObject.XWorldLimits[0]) + decimal.Parse(tileObject.XWorldLimits[1])) * 0.5m;
        decimal y = (decimal.Parse(tileObject.YWorldLimits[0]) + decimal.Parse(tileObject.YWorldLimits[1])) * 0.5m;
        tileRealPosition = new Vector3Decimal(x, decimal.Parse(tileObject.MinAlt), y);

        terrainSize = gameObject.GetComponent<Terrain>().terrainData.size;
        globalScaleFactor = (decimal.Parse(tileObject.XWorldLimits[1]) - decimal.Parse(tileObject.XWorldLimits[0])) / ((decimal)terrainSize.x*totalNumberOfTiles);
        globalHeightScaleFactor = (decimal.Parse(tileObject.MaxAlt) - decimal.Parse(tileObject.MinAlt)) / ((decimal)terrainSize.y);
        terrainPosition = transform.position + terrainSize / 2;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
