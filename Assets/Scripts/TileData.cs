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
        //tileFileName = gameObject.name + ".txt";
        //string path = "Assets/Tiles/" + tileFileName;
        //StreamReader sr = File.OpenText(path);
        //string json = sr.ReadToEnd();
        //tileFileName = gameObject.name;
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
		
	/*	
		Debug.Log("tileObject.XWorldLimits[0]: "+tileObject.XWorldLimits[0]);
		Debug.Log("tileObject.XWorldLimits[1]: "+tileObject.XWorldLimits[1]);

		Debug.Log("tileObject.YWorldLimits[0]: "+tileObject.YWorldLimits[0]);
		Debug.Log("tileObject.YWorldLimits[1]: "+tileObject.YWorldLimits[1]);

		
		Debug.Log("x: "+x);
		Debug.Log("y: "+y);
		Debug.Log("totalNumberOfTiles "+totalNumberOfTiles);
		
		Debug.Log("terrainSize: "+terrainSize);
		Debug.Log("globalScaleFactor: "+globalScaleFactor);
		Debug.Log("globalHeightScaleFactor: "+globalHeightScaleFactor);

		Debug.Log("tileRealPosition: "+tileRealPosition);
		
		
*/
    }

    // Update is called once per frame
    void Update () {
		
	}
}
