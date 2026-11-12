using UavRoutePlanner.BLL.Models.Geo;

namespace UavRoutePlanner.BLL.Services.Common;


public static class GeoMath
{
   
    public const double EarthRadiusM = 6_371_008.8;

    public static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    public static double ToDegrees(double radians) => radians * 180.0 / Math.PI;

    public static double Normalize360(double degrees)
    {
        var value = degrees % 360.0;
        return value < 0 ? value + 360.0 : value;
    }

    public static double Normalize180(double degrees)
    {
        var value = Normalize360(degrees);
        return value > 180.0 ? value - 360.0 : value;
    }

    public static double DistanceM(GeoPoint a, GeoPoint b)
    {
        var phi1 = ToRadians(a.Latitude);
        var phi2 = ToRadians(b.Latitude);
        var dPhi = phi2 - phi1;
        var dLambda = ToRadians(b.Longitude - a.Longitude);

        var h = Math.Sin(dPhi / 2) * Math.Sin(dPhi / 2)
                + Math.Cos(phi1) * Math.Cos(phi2) * Math.Sin(dLambda / 2) * Math.Sin(dLambda / 2);

        return 2 * EarthRadiusM * Math.Asin(Math.Min(1.0, Math.Sqrt(h)));
    }

    public static double InitialBearingDeg(GeoPoint a, GeoPoint b)
    {
        var phi1 = ToRadians(a.Latitude);
        var phi2 = ToRadians(b.Latitude);
        var dLambda = ToRadians(b.Longitude - a.Longitude);

        var y = Math.Sin(dLambda) * Math.Cos(phi2);
        var x = Math.Cos(phi1) * Math.Sin(phi2)
                - Math.Sin(phi1) * Math.Cos(phi2) * Math.Cos(dLambda);

        return Normalize360(ToDegrees(Math.Atan2(y, x)));
    }
}
