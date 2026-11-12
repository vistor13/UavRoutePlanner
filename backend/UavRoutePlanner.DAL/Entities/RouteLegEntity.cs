using UavRoutePlanner.DAL.Entities.CommonEntity;

namespace UavRoutePlanner.DAL.Entities;

/// <summary>Ділянка побудованого маршруту: перехід у наступну точку з вказаним курсом і часом.</summary>
public class RouteLegEntity : BaseEntity
{
    /// <summary>Порядковий номер ділянки в маршруті.</summary>
    public int Order { get; set; }

    public string PointName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public double DistanceM { get; set; }
    public double TrackDeg { get; set; }
    public double HeadingDeg { get; set; }
    public double GroundSpeedMs { get; set; }
    public double TimeS { get; set; }

    public long RouteResultId { get; set; }
    public RouteResultEntity RouteResult { get; set; } = null!;
}
