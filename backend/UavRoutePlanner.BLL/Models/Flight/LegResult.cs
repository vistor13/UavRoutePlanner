namespace UavRoutePlanner.BLL.Models.Flight;


public sealed record LegResult(
    double DistanceM,
    double TrackDeg,
    double CorrectionDeg,
    double HeadingDeg,
    double GroundSpeedMs,
    double TimeS);
