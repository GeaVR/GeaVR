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

[System.Serializable]
public class TileObject {
    public string[] XWorldLimits, YWorldLimits, XWorldLimitsDD, YWorldLimitsDD, XWorldLimitsUU, YWorldLimitsUU, RasterSize, XIntrinsicLimits, YIntrinsicLimits;
    public string CellExtentInWorldX, CellExtentInWorldY, RasterExtentInWorldX, RasterExtentInWorldY, MaxAlt, MinAlt, AltDifference, PixelSize;
    public string RasterInterpretation, ColumnsStartFrom, RowsStartFrom, TransformationType, CoordinateSystemType;

    public TileObject()
    {
    }

    public override string ToString()
    {
      
        return "{\n" +
        "\"XWorldLimits\": " + XWorldLimits + ",\n" +
        "\"YWorldLimits\": " + YWorldLimits + ",\n" +
        "\"XWorldLimitsDD\": " + XWorldLimitsDD + ",\n" +
        "\"YWorldLimitsDD\": " + YWorldLimitsDD + ",\n" +
        "\"XWorldLimitsUU\": " + XWorldLimitsUU + ",\n" +
        "\"YWorldLimitsUU\": " + YWorldLimitsUU + ",\n" +
        "\"RasterSize\": " + RasterSize + ",\n" +
        "RasterInterpretation\": " + RasterInterpretation + ",\n" +
        "ColumnsStartFrom\": " + ColumnsStartFrom + ",\n" +
        "RowsStartFrom\": " + RowsStartFrom + ",\n" +
        "CellExtentInWorldX\": " + CellExtentInWorldX + ",\n" +
        "CellExtentInWorldY\": " + CellExtentInWorldY + ",\n" +
        "RasterExtentInWorldX\": " + RasterExtentInWorldX + ",\n" +
        "RasterExtentInWorldY\": " + RasterExtentInWorldY + ",\n" +
        "XIntrinsicLimits\": " + XIntrinsicLimits + ",\n" +
        "YIntrinsicLimits\": " + YIntrinsicLimits + ",\n" +
        "TransformationType\": " + TransformationType + ",\n" +
        "CoordinateSystemType\": " + CoordinateSystemType + ",\n" +
        "MaxAlt\": " + MaxAlt + ",\n" +
        "MinAlt\": " + MinAlt + ",\n" +
        "AltDifference\": " + AltDifference + ",\n" +
        "PixelSize\": " + PixelSize + "\n" +
        "}";
       
    }

}
