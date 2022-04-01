/******************************************************************************
 *
 * Name:        TerrainLoader.cs
 * Project:     3dTeLC
 * Author:      Martin Kearl 2019
 * Institution: University of Portsmouth, UK
 *
 ******************************************************************************
 * Copyright (c) 2016-2020
 * license notice to be determined 
 *****************************************************************************/
 using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SharpKml.Dom;
using SharpKml.Engine;
using System.Xml;
using System.Globalization;
using System.Xml.Linq;

public struct StreamedTerrainCellData
{
    public string Name;
    public string Hash;
    public string Path;
    public Vector3 Position;
    public bool isLoaded;
    public bool isLoading;
}

/// <summary>
/// Loads and stores the required data for streamed terrains
/// 
/// loads from:
///     scene.xml
///     tiles.xml
///     doc.kml
/// 
/// </summary>
public class StreamedTerrainData
{
    Decimal west_dd = 0.0m;
    Decimal east_dd = 0.0m;
    Decimal south_dd = 0.0m;
    Decimal north_dd = 0.0m;

    Decimal west_uu = 0.0m;
    Decimal east_uu = 0.0m;
    Decimal south_uu = 0.0m;
    Decimal north_uu = 0.0m;

    Decimal alt_min_dd = 0.0m;
    Decimal alt_max_dd = 0.0m;
    Decimal alt_min_uu = 0.0m;
    Decimal alt_max_uu = 0.0m;

    Decimal offset_alt = 0m;


    /// <summary>
    /// 
    /// </summary>
    public float gridSize = 512;

    /// <summary>
    /// 
    /// </summary>
    public List<StreamedTerrainCellData> CellList = new List<StreamedTerrainCellData>();

    /// <summary>
    /// 
    /// </summary>
    public Vector3 startPosition = new Vector3();

    /// <summary>
    /// 
    /// </summary>
    private bool usingLegacyLoading = true;
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsUsingLegacyLoading()
    {
        return usingLegacyLoading;
    }

    /// <summary>
    ///  Load 
    /// </summary>
    public StreamedTerrainData GetDeepCopy()
    {
        StreamedTerrainData newData = new StreamedTerrainData();

        newData.gridSize = gridSize;
        newData.usingLegacyLoading = usingLegacyLoading;
        newData.CellList = new List<StreamedTerrainCellData>();

        foreach (StreamedTerrainCellData cell in CellList )
        {
            StreamedTerrainCellData newCell = new StreamedTerrainCellData();

            newCell.Name        = cell.Name;
            newCell.Path        = cell.Path;
            newCell.Position    = cell.Position;
            newCell.isLoaded    = cell.isLoaded;
            newCell.isLoading   = cell.isLoading;

            newData.CellList.Add(newCell);
        }

        return newData;
    }

    /// <summary>
    ///  Load 
    /// </summary>
    /// <param name="importLocation"></param>
    public void LoadDocKML(string importLocation)
    {        
        Debug.Log("loading doc.kml");
        XDocument newXml;

        try
        {
            newXml = XDocument.Load(importLocation + "\\doc.kml");
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
            newXml = null;
        }

        if (newXml != null)
        {
            XNamespace ns = newXml.Root.Name.Namespace;
            XElement elem = newXml.Element(ns + "kml")
                .Element(ns + "Folder")
                .Element(ns + "Region")
                .Element(ns + "LatLonAltBox");

            if (elem != null)
            {
                west_dd     = Decimal.Parse(elem.Element(ns + "west").Value, new CultureInfo("en-GB"));
                east_dd     = Decimal.Parse(elem.Element(ns + "east").Value, new CultureInfo("en-GB"));
                south_dd    = Decimal.Parse(elem.Element(ns + "south").Value, new CultureInfo("en-GB"));
                north_dd    = Decimal.Parse(elem.Element(ns + "north").Value, new CultureInfo("en-GB"));

                alt_min_dd  = Decimal.Parse(elem.Element(ns + "minAltitude").Value, new CultureInfo("en-GB"));
                alt_max_dd  = Decimal.Parse(elem.Element(ns + "maxAltitude").Value, new CultureInfo("en-GB"));
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="importLocation"></param>
    public void LoadTilesXML(string importLocation)
    {
        Debug.Log("loading tiles.xml");
        XmlDocument newXml = new XmlDocument();
        try
        {
            newXml.Load(importLocation + "\\tiles.xml");
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
            newXml = null;
        }

        if (newXml != null)
        {
            XmlNode root = newXml.DocumentElement;
            var nodeList = root.SelectNodes("/document/folder/extent");
            if (nodeList.Count > 0)
            {
                west_uu = Decimal.Parse(nodeList[0]["xmin"].InnerText, new CultureInfo("en-GB"));
                east_uu = Decimal.Parse(nodeList[0]["xmax"].InnerText, new CultureInfo("en-GB"));
                south_uu = Decimal.Parse(nodeList[0]["ymin"].InnerText, new CultureInfo("en-GB"));
                north_uu = Decimal.Parse(nodeList[0]["ymax"].InnerText, new CultureInfo("en-GB"));

                alt_min_uu = Decimal.Parse(nodeList[0]["zmin"].InnerText, new CultureInfo("en-GB"));
                alt_max_uu = Decimal.Parse(nodeList[0]["zmax"].InnerText, new CultureInfo("en-GB"));
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="importLocation"></param>
    public void LoadSceneXML(string importLocation, string filename = "scene.xml")
    {
        // open scene xml
        XmlDocument sceneXml = new XmlDocument();
        try
        {
            sceneXml.Load(Path.Combine(importLocation, filename)); // importLocation + "\\scene.xml"
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
            sceneXml = null;
        }

        if (sceneXml != null)
        {
            XmlNode root = sceneXml.DocumentElement;

            // loading type
            XmlNode legacy = root.SelectSingleNode("/document/using_legacy_loading");
            if (legacy != null)
            {
                bool result;
                if (bool.TryParse(legacy.InnerText, out result))
                {
                    usingLegacyLoading = result;
                }
                else
                {
                    usingLegacyLoading = true; // flag invalid
                }
            }
            else
            {
                usingLegacyLoading = true; // flag not present
            }

            // 
            XmlNode grid = root.SelectSingleNode("/document/grid_scale");
            if (grid != null)
            {
                float result;
                if (float.TryParse(grid.InnerText, out result))
                {
                    gridSize = result;
                }
            }

            var nodeList = root.SelectNodes("/document/folder");

            if (nodeList.Count > 0)
            {
                offset_alt = Decimal.Parse(nodeList[0]["offset"].InnerText, new CultureInfo("en-GB"));
                var positionNodes = nodeList[0]["position"];
                float x = float.Parse(positionNodes["x"].InnerText, new CultureInfo("en-GB"));
                float y = float.Parse(positionNodes["y"].InnerText, new CultureInfo("en-GB"));
                float z = float.Parse(positionNodes["z"].InnerText, new CultureInfo("en-GB"));

                startPosition = new Vector3(x, y + 4, z);
            }

            var tileList = root.SelectNodes("/document/tiles");

            // fill tile list 
            if (tileList.Count > 0)
            {
                var objectList = tileList[0].SelectNodes("object");

                for (int i = 0; i < objectList.Count; i++)
                {
                    XmlNode currentNode = objectList[i];
                    XmlNode pathNode = currentNode.SelectSingleNode("path");
                    XmlNode positionNode = currentNode.SelectSingleNode("position");

                    // get path
                    string name = pathNode.InnerText;
                    string hash = name;
                    if (hash.EndsWith("Low"))
                    {
                        hash = hash.Remove(name.Length - 3);
                        Debug.Log(hash);
                    }

                    // get position
                    float x = float.Parse(positionNode["x"].InnerText, new CultureInfo("en-GB"));
                    float y = float.Parse(positionNode["y"].InnerText, new CultureInfo("en-GB"));
                    float z = float.Parse(positionNode["z"].InnerText, new CultureInfo("en-GB"));
                    Vector3 pos = new Vector3(x, y, z);

                    // create new cell
                    StreamedTerrainCellData newCell = new StreamedTerrainCellData();
                    newCell.Name = name + ".SOBJ";
                    newCell.Hash = hash;
                    newCell.Path = Path.Combine(importLocation, newCell.Name);
                    newCell.Position = pos;
                    newCell.isLoaded = false;
                    newCell.isLoading = false;

                    // add to cell list
                    CellList.Add(newCell);
                }
            }

            // first time CellList sort
            // sort list by distance            
            SortCellList(startPosition);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateStaticGPSLatLonAltBox()
    {
        PositionController.SetStaticGPSLatLonAltBox(
            west_dd,
            east_dd,
            south_dd,
            north_dd,

            west_uu,
            east_uu,
            south_uu,
            north_uu,

            alt_min_dd,
            alt_max_dd,
            alt_min_uu,
            alt_max_uu,

            offset_alt
        );
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetLoaded(int index, bool isLoaded)
    {
        StreamedTerrainCellData cell = CellList[index];
        StreamedTerrainCellData newCell = new StreamedTerrainCellData();

        newCell.Name = cell.Name;
        newCell.Hash = cell.Hash;
        newCell.Path = cell.Path;
        newCell.Position = cell.Position;
        newCell.isLoaded = isLoaded;
        newCell.isLoading = cell.isLoading;

        CellList[index] = newCell;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetLoading(int index, bool isLoading)
    {
        StreamedTerrainCellData cell = CellList[index];
        StreamedTerrainCellData newCell = new StreamedTerrainCellData();

        newCell.Name = cell.Name;
        newCell.Hash = cell.Hash;
        newCell.Path = cell.Path;
        newCell.Position = cell.Position;
        newCell.isLoaded = cell.isLoaded;
        newCell.isLoading = isLoading;

        CellList[index] = newCell;
    }

    /// <summary>
    /// Sorts the CellList by distance to position
    /// Slow, use sparingly
    /// </summary>
    /// <param name="centrePoint"></param>
    public void SortCellList(Vector3 centrePoint)
    {
        CellList.Sort((x, y) =>
        {
            float manhattanX = Math.Abs(x.Position.x - centrePoint.x) + Math.Abs(x.Position.z - centrePoint.z);
            float manhattanZ = Math.Abs(y.Position.x - centrePoint.x) + Math.Abs(y.Position.z - centrePoint.z);
            return manhattanX.CompareTo(manhattanZ);
        });
    }

    /// <summary>
    /// Prints the current GPS coordinate data to the debug log
    /// </summary>
    public void StaticGPSLatLonAltDebugMessage()
    {
        Debug.Log(west_dd); 
        Debug.Log(east_dd); 
        Debug.Log(south_dd);
        Debug.Log(north_dd);
        Debug.Log(west_uu); 
        Debug.Log(east_uu); 
        Debug.Log(south_uu); 
        Debug.Log(north_uu); 
        Debug.Log(alt_min_dd);
        Debug.Log(alt_max_dd); 
        Debug.Log(alt_min_uu);
        Debug.Log(alt_max_uu); 
        Debug.Log(offset_alt); 
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="importLocation"></param>
    /// <returns></returns>
    public static StreamedTerrainData LoadStreamedTerrainData( string importLocation )
    {
        StreamedTerrainData terrain = new StreamedTerrainData();

        // load relevant files and populate StreamedTerrainData object
        terrain.LoadDocKML(importLocation);
        terrain.LoadTilesXML(importLocation);
        terrain.LoadSceneXML(importLocation);

        // update 
        terrain.UpdateStaticGPSLatLonAltBox();

        return terrain;
    }    
}

