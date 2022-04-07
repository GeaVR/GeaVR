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
   
    public static GameObject WalkingMasterObject;
    public static GameObject FlyingMasterObject;
    public static GameObject DroneMasterObject;
    
    // Use this for initialization
    void Start()
    {
       
    }

    [System.Obsolete("CalculateHeadingFromPositions is deprecated, please use PositionController.CalculateHeadingFromPositions instead.")]
    public static float CalculateHeadingFromPositions(Vector3 Pos1, Vector3 Pos2)
    {
        
        return PositionController.CalculateHeadingFromPositions(Pos1, Pos2);
    }

    [System.Obsolete("CalculateHeading is deprecated, please use PositionController.CalculateHeading instead.")]
    public static float CalculateHeading(Vector3 DirectionVector)
    {
       
        return PositionController.CalculateHeading(DirectionVector);
    }


    [System.Obsolete("CalculateInclinationFromPositions is deprecated, please use PositionController.CalculateInclinationFromPositions instead.")]
    public static float CalculateInclinationFromPositions(Vector3 Pos1, Vector3 Pos2)
    {
       
        return PositionController.CalculateInclinationFromPositions(Pos1, Pos2);

    }

    [System.Obsolete("CalculateInclination is deprecated, please use PositionController.CalculateInclination instead.")]
    public static float CalculateInclination(Vector3 DirectionVector)
    {
        
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

    
}
    