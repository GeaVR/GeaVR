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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VirtualMeter : MonoBehaviour
{
    /*
    public GameObject refTile = null;
    public static TileData td = null;

    // with the new .obj system the tiledata system seems unneeded
    // could we just manually set the offset and scaling factor once, in this script

    //positions then use CalculateRealPositionOfPoint() to get correct gps coordinates

    // real positions are then used for all calculations. 

    public static Vector3 terrainPosition = new Vector3(0.0f, 0.0f, 0.0f);
    public static decimal globalScaleFactor = 1.0m;
    public static decimal globalHeightScaleFactor = 1.0m;

    public static Vector3Decimal tileRealPosition = new Vector3Decimal(0.0m, 0.0m, 0.0m);
    */
    public static GameObject WalkingMasterObject;
    public static GameObject FlyingMasterObject;
    public static GameObject DroneMasterObject;
    
    // Use this for initialization
    void Start()
    {
        /*
        // a messy solution, we need to unify how coordinates are calculated. 
        WalkingMasterObject = GameObject.Find("FPSController");
        FlyingMasterObject = GameObject.Find("TopCamera");
        DroneMasterObject = GameObject.Find("DroneModeCamera");

        if (refTile != null)
        {             
            td = refTile.GetComponent<TileData>();

            terrainPosition             = td.terrainPosition;
            globalScaleFactor           = td.globalScaleFactor;            
            globalHeightScaleFactor     = td.globalHeightScaleFactor;
            tileRealPosition            = td.tileRealPosition;
        }
        */
    }

    [System.Obsolete("CalculateHeadingFromPositions is deprecated, please use PositionController.CalculateHeadingFromPositions instead.")]
    public static float CalculateHeadingFromPositions(Vector3 Pos1, Vector3 Pos2)
    {
        /*
        Vector3 delta = Pos2 - Pos1;
        float heading = Vector3.SignedAngle(Vector3.forward, new Vector3(-delta.x, 0, -delta.z), Vector3.up);
        return (heading > 0) ? heading : (360 + heading);
        */

        return PositionController.CalculateHeadingFromPositions(Pos1, Pos2);
    }

    [System.Obsolete("CalculateHeading is deprecated, please use PositionController.CalculateHeading instead.")]
    public static float CalculateHeading(Vector3 DirectionVector)
    {
        /*
        float heading = Vector3.SignedAngle(Vector3.forward, -DirectionVector, Vector3.up);
        return (heading > 0) ? heading : (360 + heading);
        */
        return PositionController.CalculateHeading(DirectionVector);
    }


    [System.Obsolete("CalculateInclinationFromPositions is deprecated, please use PositionController.CalculateInclinationFromPositions instead.")]
    public static float CalculateInclinationFromPositions(Vector3 Pos1, Vector3 Pos2)
    {
        /*
        Vector3 DirectionVector = Pos1 - Pos2;
        Vector3 FlattenedDirectionVector = Vector3.ProjectOnPlane(DirectionVector, Vector3.up).normalized;
        return Vector3.SignedAngle(DirectionVector, FlattenedDirectionVector, Vector3.Cross(DirectionVector, FlattenedDirectionVector));
        */
        return PositionController.CalculateInclinationFromPositions(Pos1, Pos2);

    }

    [System.Obsolete("CalculateInclination is deprecated, please use PositionController.CalculateInclination instead.")]
    public static float CalculateInclination(Vector3 DirectionVector)
    {
        /*
        Vector3Decimal temp;
        temp = (Vector3Decimal.Parse(point) - Vector3Decimal.Parse(terrainPosition));
        temp.x *= globalScaleFactor;
        temp.z *= globalScaleFactor;
        temp.y *= globalHeightScaleFactor;
        temp += tileRealPosition;        
        return temp;  
        */
        return PositionController.CalculateInclination(DirectionVector);

    }


    [System.Obsolete("CalculateRealPositionOfPoint is deprecated, please use PositionController.CalculateRealPositionOfPoint instead.")]
    public static Vector3Decimal CalculateRealPositionOfPoint(Vector3 point)
    {
        return PositionController.CalculateRealPositionOfPoint(point);

    }

    [System.Obsolete("CalculateGPSPosition is deprecated, please use PositionController.CalculateGPSPosition instead.")]
    public static Vector3Decimal CalculateGPSPosition(Vector3 point)
    {
        return PositionController.CalculateGPSPosition(point);
    }

    [System.Obsolete("CalculateRealDistance is deprecated, please use PositionController.CalculateRealDistance instead.")]
    public static decimal CalculateRealDistance(Vector3 point1, Vector3 point2)
    {
        return PositionController.CalculateRealDistance(point1, point2);

    }

    [System.Obsolete("CalculateRealDistance2D is deprecated, please use PositionController.CalculateRealDistance2D instead.")]
    public static decimal CalculateRealDistance2D(Vector3 point1, Vector3 point2)
    {
        return PositionController.CalculateRealDistance2D(point1, point2);

    }

    [System.Obsolete("CalculateRealDistance is deprecated, please use PositionController.CalculateRealDistance instead.")]
    public static decimal CalculateRealDistance(Vector3Decimal point1, Vector3Decimal point2)
    {
        return PositionController.CalculateRealDistance(point1, point2);
    }

    [System.Obsolete("CalculateGroundDistance is deprecated, please use PositionController.CalculateGroundDistance instead.")]
    public static decimal CalculateGroundDistance(Vector3 start, Vector3 end, LineRenderer lineRenderer = null, int maxRays = int.MaxValue)
    {
        return PositionController.CalculateGroundDistance(start, end, lineRenderer, maxRays);
    }

    [System.Obsolete("CalculateVerticalRealDistance2D is deprecated, please use PositionController.CalculateVerticalRealDistance2D instead.")]
    public static decimal CalculateVerticalRealDistance2D(Vector3 point1, Vector3 point2)
    {
        return PositionController.CalculateVerticalRealDistance2D(point1, point2);
    }


    [System.Obsolete("SetStaticGPSLatLonAltBox is deprecated, please use PositionController.SetStaticGPSLatLonAltBox instead.")]
    public static void SetStaticGPSLatLonAltBox(
        Decimal _west_dd,
        Decimal _east_dd,
        Decimal _south_dd,
        Decimal _north_dd,

        Decimal _west_uu,
        Decimal _east_uu,
        Decimal _south_uu,
        Decimal _north_uu,

        Decimal _alt_min_dd,
        Decimal _alt_max_dd,
        Decimal _alt_min_uu,
        Decimal _alt_max_uu,

        Decimal _offset_alt)
    {
        PositionController.SetStaticGPSLatLonAltBox(
        _west_dd,
        _east_dd,
        _south_dd,
        _north_dd,
        _west_uu,
        _east_uu,
        _south_uu,
        _north_uu,
        _alt_min_dd,
        _alt_max_dd,
        _alt_min_uu,
        _alt_max_uu,
        _offset_alt);
    }

    /*
     * OLD
    public static Vector3Decimal CalculateUnityPosition(Vector3 point)
    {
        return WalkingMasterObject.GetComponent<PositionController>().CalculateUnityPosition(point);
    }


    public static Vector3Decimal CalculateGPSPosition(Vector3 point)
    {
        return WalkingMasterObject.GetComponent<PositionController>().CalculateRealPosition(point);        
    }


    public static decimal CalculateRealDistance(Vector3 point1, Vector3 point2)
    {
        // calculate real positions
        Vector3Decimal Pos1 = VirtualMeter.CalculateRealPositionOfPoint(point1);
        Vector3Decimal Pos2 = VirtualMeter.CalculateRealPositionOfPoint(point2);

        return CalculateRealDistance(Pos1, Pos2);
    }

    public static decimal CalculateRealDistance(Vector3Decimal point1, Vector3Decimal point2)
    {     
        // pythagoras 
        decimal a = point1.x - point2.x;
        decimal b = point1.y - point2.y;
        decimal c = point1.z - point2.z;
        
        return (decimal)System.Math.Sqrt((double)(a * a + b * b + c * c)); // lose precision :/ 
    }

    public static decimal CalculateVerticalRealDistance2D(Vector3 point1, Vector3 point2)
    {
        // calculate real positions
        Vector3Decimal Pos1 = VirtualMeter.CalculateRealPositionOfPoint(point1);
        Vector3Decimal Pos2 = VirtualMeter.CalculateRealPositionOfPoint(point2);

        return CalculateVerticalRealDistance2D(Pos1, Pos2);
    }
    public static decimal CalculateRealDistance2D(Vector3 point1, Vector3 point2)
    {
        // calculate real positions
        Vector3Decimal Pos1 = VirtualMeter.CalculateRealPositionOfPoint(point1);
        Vector3Decimal Pos2 = VirtualMeter.CalculateRealPositionOfPoint(point2);

        return CalculateRealDistance2D(Pos1, Pos2);
    }
    public static decimal CalculateRealDistance2D(Vector3Decimal point1, Vector3Decimal point2)
    {
        // pythagoras 
        decimal a = point1.x - point2.x;
        decimal b = point1.z - point2.z;


      //  return (decimal)(Mathf.Max((float)point1.x, (float)point2.x) - Mathf.Min((float)point1.x, (float)point2.x)); 
        // lose precision :/ 
                                                                                                                    
         return (decimal)System.Math.Sqrt((double)(a * a + b * b)); // lose precision :/ 
    }

    public static decimal CalculateVerticalRealDistance2D(Vector3Decimal point1, Vector3Decimal point2)
    {



        return (decimal)(Mathf.Max((float)point1.y, (float)point2.y) - Mathf.Min((float)point1.y, (float)point2.y)); // lose precision :/ 
    }
     public static decimal CalculateGroundDistance(Vector3 start, Vector3 end, LineRenderer lineRenderer = null, int maxRays = int.MaxValue)
    {
        Vector3 rayVector = new Vector3(0.0f, -1.0f, 0.0f);
        RaycastHit hit;

        //calculate initial positions for points
        Vector3 startPoint = start;
        Vector3 endPoint = end;

        float linearDistance = Vector3.Distance(startPoint, endPoint);
        float linearStep = 0.01f;

        //calculate steps
        int stepCount = (int)(linearDistance / linearStep);

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = System.Math.Min(stepCount, maxRays);
        }
       
        // map projection
        startPoint.y = 1000;
        endPoint.y = 1000;

        // temp position
        Vector3 previousPoint = start;
        float accumulatedDistance = 0;
        int rayMisses = 0;

        for (int i = 0; i < stepCount; i++)
        {
            // step along line between points
            float lerpAlpha = (float)i / (float)stepCount;
            Vector3 castOrigin = Vector3.Lerp(startPoint, endPoint, lerpAlpha);

            // cast ray down from point
            if (Physics.Raycast(castOrigin, rayVector, out hit, 5000))
            {
                // if hit add to distance
                accumulatedDistance += (float)VirtualMeter.CalculateRealDistance(previousPoint, hit.point);
                previousPoint = hit.point;

                if (lineRenderer != null)
                {
                    lineRenderer.SetPosition(i, hit.point);
                }                
            }
            else // ray miss
            {
                rayMisses++;
            }
        }

        // decrement renderer by appropriate amount
        lineRenderer.positionCount = lineRenderer.positionCount - rayMisses;

        //End process
        decimal decimalDistance = (decimal)accumulatedDistance;
        return decimalDistance;        
    }
    */
}
    