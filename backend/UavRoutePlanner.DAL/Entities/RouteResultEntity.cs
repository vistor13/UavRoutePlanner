using UavRoutePlanner.DAL.Entities.CommonEntity;
using UavRoutePlanner.DAL.Entities.Configuration;
using UavRoutePlanner.DAL.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace UavRoutePlanner.DAL.Entities;

[EntityTypeConfiguration(typeof(RouteResultEntityTypeConfiguration))]
public class RouteResultEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public WindMode WindMode { get; set; }

    public DateTime DepartureTimeUtc { get; set; }

    public double WindSpeedMs { get; set; }
    public double WindDirectionFromDeg { get; set; }

    public int VisitedObjectsCount { get; set; }

    public double TotalTimeS { get; set; }

    public ICollection<RouteLegEntity> Legs { get; set; } = new List<RouteLegEntity>();

    public long MissionId { get; set; }
    public MissionEntity Mission { get; set; } = null!;
}
