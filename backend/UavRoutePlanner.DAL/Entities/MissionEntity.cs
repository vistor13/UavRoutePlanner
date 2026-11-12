using System.ComponentModel.DataAnnotations;
using UavRoutePlanner.DAL.Entities.CommonEntity;
using UavRoutePlanner.DAL.Entities.Configuration;
using Microsoft.EntityFrameworkCore;

namespace UavRoutePlanner.DAL.Entities;


[EntityTypeConfiguration(typeof(MissionEntityTypeConfiguration))]
public class MissionEntity : BaseEntity
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public GeoPointEntity Start { get; set; } = new();

    public GeoPointEntity Finish { get; set; } = new();

    public UavParametersEntity Uav { get; set; } = null!;

    public AlgorithmSettingsEntity Algorithm { get; set; } = null!;

    public ICollection<SurveyObjectEntity> Objects { get; set; } = new List<SurveyObjectEntity>();

    public long UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public long? UavProfileId { get; set; }
    public UavProfileEntity? UavProfile { get; set; }

    public ICollection<RouteResultEntity> Results { get; set; } = new List<RouteResultEntity>();
}
