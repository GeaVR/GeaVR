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
