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
using UnityEngine.UI;
using System;

public struct Vector3Decimal
{
    public decimal x, y, z;
    public static readonly Vector3Decimal zero = new Vector3Decimal(0, 0, 0);
    public Vector3Decimal(decimal x, decimal y, decimal z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static explicit operator Vector3(Vector3Decimal a)
    {
        return new Vector3((float)a.x, (float)a.y, (float)a.z);
    }

    public static Vector3Decimal Parse(Vector3 v)
    {
        return new Vector3Decimal((decimal)v.x, (decimal)v.y, (decimal)v.z);
    }

    public static Vector3Decimal operator +(Vector3Decimal a, Vector3Decimal b)
    {
        return new Vector3Decimal(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vector3Decimal operator -(Vector3Decimal a, Vector3Decimal b)
    {
        return new Vector3Decimal(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static Vector3Decimal operator -(Vector3Decimal a)
    {
        return new Vector3Decimal(-a.x, -a.y, -a.z);
    }

    public static Vector3Decimal operator *(Vector3Decimal a, decimal d)
    {
        return new Vector3Decimal(a.x * d, a.y * d, a.z * d);
    }

    public static Vector3Decimal operator *(decimal d, Vector3Decimal a)
    {
        return new Vector3Decimal(a.x * d, a.y * d, a.z * d);
    }

    public static Vector3Decimal operator /(Vector3Decimal a, decimal d)
    {
        return new Vector3Decimal(a.x / d, a.y / d, a.z / d);
    }

    public static bool operator ==(Vector3Decimal lhs, Vector3Decimal rhs)
    {
        return (lhs.x == rhs.x) && (lhs.y == rhs.y) && (lhs.z == rhs.z);
    }

    public static bool operator !=(Vector3Decimal lhs, Vector3Decimal rhs)
    {
        return !(lhs == rhs);
    }

    public override bool Equals(object o)
    {
        return this == (Vector3Decimal)o;
    }

    public static decimal Distance(Vector3Decimal a, Vector3Decimal b)
    {
        Vector3 av = (Vector3)a;
        Vector3 bv = (Vector3)b;
        return (decimal)Vector3.Distance(av, bv);
    }

    public static decimal Magnitude(Vector3Decimal v)
    {
        return Vector3Decimal.Distance(Vector3Decimal.zero, v);
    }

    public override string ToString()
    {
        return "{ " + this.x.ToString() + " " + this.y.ToString() + " " + this.z.ToString() + " }";
    }

    public string ToString(string format)
    {
        return "{ " + this.x.ToString(format) + " " + this.y.ToString(format) + " " + this.z.ToString(format) + " }";
    }
};

public class PositionController : MonoBehaviour
{
    public Vector3Decimal playerRealPosition;
    private Vector3Decimal newPlayerRealPosition;

    public List<GameObject> tilesColliding;
    public TileObject tileObject;

    public TileData td;


    public GameObject refTile;
    public string tileFileName;

    private static decimal west_dd;
    private static decimal east_dd;
    private static decimal south_dd;
    private static decimal north_dd;

    private static decimal west_uu;
    private static decimal east_uu;
    private static decimal south_uu;
    private static decimal north_uu;

    private static decimal alt_min_uu;
    private static decimal alt_max_uu;
    private static decimal alt_min_dd;
    private static decimal alt_max_dd;

    private static decimal delta_uu_ew;
    private static decimal delta_dd_ew;
    private static decimal conversionValue_ew;

    private static decimal delta_uu_ns;
    private static decimal delta_dd_ns;
    private static decimal conversionValue_ns;

    private static decimal delta_uu_z;
    private static decimal delta_dd_z;
    private static decimal conversionValue_alt;

    private static decimal offset_alt;
    private static decimal pixelSize;




    // Use this for initialization
    void Start()
    {
        playerRealPosition = new Vector3Decimal();
        newPlayerRealPosition = new Vector3Decimal();
    }

    void Update()
    {
        newPlayerRealPosition = CalculateGPSPosition(transform.position);

        PositionSingleton.playerRealElevation = newPlayerRealPosition.y;
        playerRealPosition = newPlayerRealPosition;
        PositionSingleton.playerRealPosition = playerRealPosition;


    }

    /**
	 * Calculates the heading from pos1 to pos2 
	 *
	 * @param pos1 The first position in Unity coordinates
	 * @param pos2 The second position in Unity coordinates
     * @return float containing the heading in degrees
	 */
    public static float CalculateHeadingFromPositions(Vector3 Pos1, Vector3 Pos2)
    {
        Vector3 delta = Pos2 - Pos1;
        float heading = Vector3.SignedAngle(Vector3.forward, new Vector3(-delta.x, 0, -delta.z), Vector3.up);
        return (heading > 0) ? heading : (360 + heading);
    }

    /**
	 * Calculates the heading of a direction vector
	 *
	 * @param DirectionVector The vector used to calculate the heading
     * @return float containing the heading in degrees
	 */
    public static float CalculateHeading(Vector3 DirectionVector)
    {
        float heading = Vector3.SignedAngle(Vector3.forward, -DirectionVector, Vector3.up);
        return (heading > 0) ? heading : (360 + heading);
    }

    /**
	 * description
	 *
	 * @param 
     * @return 
	 */
    public static float CalculateInclinationFromPositions(Vector3 Pos1, Vector3 Pos2)
    {
        Vector3 DirectionVector = Pos1 - Pos2;
        Vector3 FlattenedDirectionVector = Vector3.ProjectOnPlane(DirectionVector, Vector3.up).normalized;
        return Vector3.SignedAngle(DirectionVector, FlattenedDirectionVector, Vector3.Cross(DirectionVector, FlattenedDirectionVector));
    }

    /**
	 * description
	 *
	 * @param 
     * @return 
	 */
    public static float CalculateInclination(Vector3 DirectionVector)
    {
        Vector3 FlattenedDirectionVector = Vector3.ProjectOnPlane(DirectionVector, Vector3.up).normalized;
        return Vector3.SignedAngle(DirectionVector, FlattenedDirectionVector, Vector3.Cross(DirectionVector, FlattenedDirectionVector));
    }


    public static Vector3Decimal CalculateRealPositionOfPoint(Vector3 point)
    {
        return CalculateGPSPosition(point);
    }

    public static Vector3Decimal CalculateGPSPosition(Vector3 point)
    {
        Vector3Decimal newRealPosition = Vector3Decimal.Parse(point);

		newRealPosition += Vector3Decimal.Parse(WorldRebaser.accumulatedOffset);
        newRealPosition.x = ((newRealPosition.x - west_uu) * conversionValue_ew) + west_dd;

        newRealPosition.z = ((newRealPosition.z - south_uu) * conversionValue_ns) + south_dd;
        newRealPosition.y = ((newRealPosition.y - alt_min_uu) * conversionValue_alt) + alt_min_dd + offset_alt;

        return newRealPosition;
    }


    /**
	 * Calculate euclidian distance between 2 points
	 *
	 * @param pos1
     * @param pos2
     * @return distance between the points
	 */
    public static decimal CalculateRealDistance(Vector3 point1, Vector3 point2)
    {
        // calculate real positions
        Vector3Decimal Pos1 = Vector3Decimal.Parse(point1);
        Vector3Decimal Pos2 = Vector3Decimal.Parse(point2);

        return CalculateRealDistance(Pos1, Pos2);
    }

    public static decimal CalculateRealDistance2D(Vector3 point1, Vector3 point2)
    {

        // calculate real positions
        Vector3Decimal Pos1 = Vector3Decimal.Parse(point1);
        Vector3Decimal Pos2 = Vector3Decimal.Parse(point2);

        // pythagoras 
        decimal a = Pos1.x - Pos2.x;
        decimal b = Pos1.z - Pos2.z;


        return (decimal)System.Math.Sqrt((double)(a * a + b * b)); // lose precision :/ 
    }

    /**
    * Calculate euclidian distance between 2 decimal points
    *
    * @param pos1
    * @param pos2
    * @return distance between the points
    */
    public static decimal CalculateRealDistance(Vector3Decimal point1, Vector3Decimal point2)
    {
        // pythagoras 
        decimal a = point1.x - point2.x;
        decimal b = point1.y - point2.y;
        decimal c = point1.z - point2.z;

        return (decimal)System.Math.Sqrt((double)(a * a + b * b + c * c)); // some precision loss, but is tiny
    }

    public static Vector3Decimal CalculateUnityPosition(Vector3Decimal point)
    {

        Debug.Log("point x:" + point.x + " - y: " + point.y + " - z: " + point.z);
        Vector3Decimal newUnityPosition = point;

        newUnityPosition.x = ((point.x - west_dd) / conversionValue_ew) + west_uu;
        //lat
        newUnityPosition.z = ((point.z - south_dd) / conversionValue_ns) + south_uu;
        //alt

        Debug.Log("((" + newUnityPosition.y + " - " + alt_min_dd + " - " + offset_alt + ") / " + conversionValue_alt + " ) - " + Math.Abs(alt_min_uu));
        //lon
        newUnityPosition.y = ((newUnityPosition.y - alt_min_dd - offset_alt) / conversionValue_alt) - Math.Abs(alt_min_uu);
		newUnityPosition -= Vector3Decimal.Parse(WorldRebaser.accumulatedOffset);

        return newUnityPosition;
    }

    public static decimal CalculateVerticalRealDistance2D(Vector3 point1, Vector3 point2)
    {


        // calculate real positions
        Vector3Decimal Pos1 = PositionController.CalculateRealPositionOfPoint(point1);
        Vector3Decimal Pos2 = PositionController.CalculateRealPositionOfPoint(point2);

        return CalculateVerticalRealDistance2D(Pos1, Pos2);
    }

    public static decimal CalculateVerticalRealDistance2D(Vector3Decimal point1, Vector3Decimal point2)
    {
        return (decimal)(Mathf.Max((float)point1.y, (float)point2.y) - Mathf.Min((float)point1.y, (float)point2.y)); // lose precision :/ 
    }

    /**
	 * Calculate the distance between 2 points following the ground topology
	 *
	 * @param start         The position in Unity coordinates to begin measuring from
     * @param end           The position in Unity coordinates to measure to
     * @param lineRenderer  *optional* A LineRenderer Object to fill with found positions along topology
     * @param maxRays       *optional* maximum number of steps between the positions, if ommited the value is calculated 
     * @param linearStep    *optional* euclidian distance between sampled points
     * @return distance between the points
	 */
    public static decimal CalculateGroundDistance(Vector3 start, Vector3 end, LineRenderer lineRenderer = null, int maxRays = int.MaxValue, float linearStep = 0.01f)
    {
        Vector3 rayVector = new Vector3(0.0f, -1.0f, 0.0f);
        RaycastHit hit;

        //calculate initial positions for points
        Vector3 startPoint = start;
        Vector3 endPoint = end;

        float linearDistance = Vector3.Distance(startPoint, endPoint);

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
                accumulatedDistance += (float)PositionController.CalculateRealDistance(previousPoint, hit.point);
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


    /**
    * Set Static Geographic coordinates for conversions
    */
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
        west_dd = _west_dd;
        east_dd = _east_dd;
        south_dd = _south_dd;
        north_dd = _north_dd;

        west_uu = _west_uu;
        east_uu = _east_uu;
        south_uu = _south_uu;
        north_uu = _north_uu;

        alt_min_dd = _alt_min_dd;
        alt_max_dd = _alt_max_dd;
        alt_min_uu = _alt_min_uu;
        alt_max_uu = _alt_max_uu;

        offset_alt = _offset_alt;

        // converted from PositionController.cs

        delta_dd_ew = east_dd - west_dd;
        delta_uu_ew = east_uu - west_uu;

        delta_uu_ew = (delta_uu_ew == 0) ? 0.000001m : delta_uu_ew; //avoid division by zero crash - could result in incorrect results
        conversionValue_ew = delta_dd_ew / delta_uu_ew;

        delta_dd_ns = north_dd - south_dd;
        delta_uu_ns = north_uu - south_uu;

        delta_uu_ns = (delta_uu_ns == 0) ? 0.000001m : delta_uu_ns; //avoid division by zero crash - could result in incorrect results
        conversionValue_ns = delta_dd_ns / delta_uu_ns;

        delta_dd_z = alt_max_dd - alt_min_dd;
        delta_uu_z = alt_max_uu - alt_min_uu;

        delta_uu_z = (delta_uu_z == 0) ? 0.000001m : delta_uu_z; //avoid division by zero crash - could result in incorrect results
        conversionValue_alt = delta_dd_z / delta_uu_z;
    }
}
