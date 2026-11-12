using UavRoutePlanner.DAL.Entities.CommonEntity;

namespace UavRoutePlanner.DAL.Entities;

public class AlgorithmSettingsEntity : BaseEntity
{
    public double Alpha { get; set; }
    public double Beta { get; set; }

    public double EvaporationRate { get; set; }

    public double PheromoneDeposit { get; set; }

    public int AntsCount { get; set; }

    public int IterationsCount { get; set; }

    public long MissionId { get; set; }
    public MissionEntity Mission { get; set; } = null!;
}
