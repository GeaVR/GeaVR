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
using SharpKml;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System;
using System.IO;
using System.Globalization;

/// <summary>
/// 
/// </summary>
public class TopographicProfileTool : Tool
{
    [HideInInspector]
    public static List<GameObject> instanceList = new List<GameObject>();
    static LineRenderer lr = null;

    public override IEnumerator ToolCoroutine()
    {
        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();
        ToolController.ToolIsCurrentlyRunning = true;

        // temporary objects
        GameObject TempPlacemark = null, LastPlaceMark = null, TempPlacemarkInfo = null;
        List<GameObject> placemarks = new List<GameObject>();


        // hold until trigger is released
        // this avoids instant placement 
        if (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
        {
            while (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.2f)
            {
                yield return wfeof;
            }
        }

        //Do twice
        for (int times = 0; times < 2; times++)
        {
            //Instantiate placemark
            TempPlacemark = Instantiate(toolControllerComponent.PlacemarkObject3);
            TempPlacemark.transform.localScale *= toolControllerComponent.MarkerScale;
            TempPlacemark.name = "topographic placemark";

            bool placemarkConfirmed = false;
            bool legalPlacemarkPlace = false;
            bool wasHolding = true;

            //Handling Placemark
            RaycastHit hit;
            Vector3Decimal realPosition;
            while (placemarkConfirmed == false)
            {
                if (checkIfToolShouldQuit())
                {
                    placemarkConfirmed = true;
                    Destroy(TempPlacemark);


                    toolControllerComponent.OculusMasterObject.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.2783019f, 0.3061422f, 1, 1);

                    foreach (var p in placemarks)
                    {
                        Destroy(p);
                    }
                    ToolController.ToolIsCurrentlyRunning = false;
                    yield break; // end coroutine early 
                }

                if (StateSingleton.stateView != StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView != StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS)
                {
                    if (wasHolding == true && OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) < 0.2f)
                        wasHolding = false;
                }

                //Trailing Waypoint
                if (Physics.Raycast(master.transform.position, directionMaster.transform.forward, out hit, 100000))
                {
                    TempPlacemark.transform.position = hit.point;
                    TempPlacemark.transform.eulerAngles = new Vector3(0f, directionMaster.transform.eulerAngles.y, directionMaster.transform.eulerAngles.z);
                    legalPlacemarkPlace = true;

                    realPosition = VirtualMeter.CalculateRealPositionOfPoint(TempPlacemark.transform.position);
                    toolControllerComponent.updateGUIMenus(
                        ((times == 0) ? "First " : "Second ") + "Placemark",
                        "\nNorthing (m): " + realPosition.z.ToString("0.000") +
                        "\nEasting (m): " + realPosition.x.ToString("0.000") +
                        "\nAltitude (m): " + realPosition.y.ToString("0.000")
                    );


                }
                else
                {
                    //If not well placed then invalidate
                    TempPlacemark.transform.position = master.transform.position + directionMaster.transform.forward * 100000;
                    realPosition = VirtualMeter.CalculateRealPositionOfPoint(TempPlacemark.transform.position);
                    toolControllerComponent.updateGUIMenus(
                        ((times == 0) ? "First " : "Second ") + "Placemark",
                        "\nNorthing (m): undefined" +
                        "\nEasting (m): undefined" +
                        "\nAltitude (m): undefined"
                    );
                    legalPlacemarkPlace = false;
                }

                //confirm waypoint
                if (
                (
                Input.GetMouseButton(0) ||
                    (
                        (StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_OCULUS || StateSingleton.stateView == StateSingleton.StateView.MODE2D_PLUS_3DVP_PLUS_OCULUS) &&
                        OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.5f && wasHolding == false
                    )
                )
                && legalPlacemarkPlace)
                {
                    placemarkConfirmed = true;
                    placemarks.Add(TempPlacemark);
                    //ToolController.ToolIsCurrentlyRunning = false;

                }
                yield return wfeof;
            }
            while (Input.GetMouseButton(0))
                yield return wfeof;
        }

        // create line renderer
        lr = placemarks[0].AddComponent<LineRenderer>() as LineRenderer;
        lr.startWidth = lr.endWidth = (float)(toolControllerComponent.MarkerScale * 0.5f);
        lr.material = toolControllerComponent.MeasurementMaterial;

        // do calculations
        toolControllerComponent.updateGUIMenus("Profile Information", "Calculating profile\nPlease wait...");
        yield return wfeof;

        // do calculations
        Vector3 uPos1 = placemarks[0].transform.position;
        Vector3 uPos2 = placemarks[1].transform.position;

        decimal realDistance = VirtualMeter.CalculateRealDistance(uPos1, uPos2);
        decimal GroundDistance = VirtualMeter.CalculateGroundDistance(uPos1, uPos2, lr);

        // create value ditionary
        Dictionary<string, decimal> dict = new Dictionary<string, decimal>()
                                            {
                                                {"Minimum_Distance(m)",             realDistance},
                                                {"Topographic_Profile_Distance(m)", GroundDistance},
                                            };


        // create tool instance - then update graphs
        GameObject NewToolInstance = toolControllerComponent.CreateToolInstance("Topographic Profile",
            //"Minimum Distance (m): " + realDistance.ToString("0.000") +
           // "\nTopographic Profile Distance (m): " + GroundDistance.ToString("0.000")
            "", "",
            Tool.toolType.PROFILE,
            dict, placemarks, DateTime.Now, true, true
            );

        NewToolInstance.GetComponent<ToolInstance>().UpdateGraphs(lr);
        toolControllerComponent.OculusMasterObject.gameObject.transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = new Color(0.2783019f, 0.3061422f, 1, 1);

        // ToolController.ToolIsCurrentlyRunning = false;        
    }

    public static void SaveSingleInstance(ToolInstance instance)
    {
        string FilePath = "Outputs/Profile";


        // CSV export

        var directory = new DirectoryInfo(Application.dataPath);
        var directoryPath = Path.Combine(directory.Parent.FullName, FilePath);

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
        }

        var path = Path.Combine(directoryPath, string.Format("{0}.csv", instance.ToolTitle.ToString() + " " + DateTime.Now.ToString("yyyyMMdd_Hmmssffff")));

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        Vector3[] points = new Vector3[lr.positionCount];
       lr.GetPositions(points);
    


        //var sr = File.CreateText(path);
        StreamWriter writer = new StreamWriter(path, true);

        string csvData = "";
        csvData = "#Lat, Lon, z, length";
        writer.WriteLine(csvData, "en-GB");
        Vector3Decimal realPosition;
        decimal realDistance2D;
        for (int i = 0; i < points.Length; ++i)
        {

            realPosition = VirtualMeter.CalculateGPSPosition(points[i]);

            realDistance2D = VirtualMeter.CalculateRealDistance2D(points[0], points[i]);


            csvData = realPosition.z.ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
            csvData += realPosition.x.ToString("0.0000000000000", new CultureInfo("en-GB")) + ",";
            csvData += realPosition.y.ToString("0.000", new CultureInfo("en-GB")) + ",";
            csvData += realDistance2D.ToString("0.000", new CultureInfo("en-GB")) ;

            writer.WriteLine(csvData, "en-GB");

        }


        writer.Close();
        





    }

    public static void SaveToFile(string FilePath, List<GameObject> PlacemarkList)
    {


        // This will be the location of the Placemark.
        var point = new Point();
        point.Coordinate = new Vector(-13.163959, -72.545992);

        // This is the Element to save to the Kml file.
        var placemark = new Placemark();
        placemark.Geometry = point;
        placemark.Name = "Machu Picchu";

        // This allows us to save and Element easily.
        KmlFile kml = KmlFile.Create(placemark, false);
        using (var stream = System.IO.File.OpenWrite("my placemark.kml"))
        {
            kml.Save(stream);
        }
    }

    public void CancelButton()
    {

       

    }
}