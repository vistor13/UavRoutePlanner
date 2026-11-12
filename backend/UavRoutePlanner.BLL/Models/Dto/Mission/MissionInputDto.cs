namespace UavRoutePlanner.BLL.Models.Dto.Mission;

public record GeoPointDto(double Latitude, double Longitude);

public record SurveyObjectInputDto(string? Name, double Latitude, double Longitude);

public record AlgorithmSettingsDto(
    double Alpha,
    double Beta,
    double EvaporationRate,
    double PheromoneDeposit,
    int AntsCount,
    int IterationsCount)
{
   
    public static AlgorithmSettingsDto Default { get; } = new(
        Alpha: 1.0,
        Beta: 1.0,
        EvaporationRate: 0.1,
        PheromoneDeposit: 0.05,
        AntsCount: 30,
        IterationsCount: 150);
}
public record MissionInputDto(
    string Name,
    GeoPointDto? Start,
    GeoPointDto? Finish,
    long UavProfileId,
    List<SurveyObjectInputDto> Objects,
    AlgorithmSettingsDto? Algorithm);
