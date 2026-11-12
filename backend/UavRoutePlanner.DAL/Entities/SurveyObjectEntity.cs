using UavRoutePlanner.DAL.Entities.CommonEntity;

namespace UavRoutePlanner.DAL.Entities;

public class SurveyObjectEntity : BaseEntity
{
    public int Number { get; set; }

    public string Name { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public long MissionId { get; set; }
    public MissionEntity Mission { get; set; } = null!;
}
