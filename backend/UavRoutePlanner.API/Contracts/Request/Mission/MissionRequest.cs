using UavRoutePlanner.BLL.Models.Dto.Mission;

namespace UavRoutePlanner.Api.Contracts.Request.Mission;

public record GeoPointRequest(double Latitude, double Longitude)
{
    public GeoPointDto ToDto() => new(Latitude, Longitude);
}

public record SurveyObjectRequest(string? Name, double Latitude, double Longitude)
{
    public SurveyObjectInputDto ToDto() => new(Name, Latitude, Longitude);
}


public record AlgorithmSettingsRequest(
    double Alpha,
    double Beta,
    double EvaporationRate,
    double PheromoneDeposit,
    int AntsCount,
    int IterationsCount)
{
    public AlgorithmSettingsDto ToDto() =>
        new(Alpha, Beta, EvaporationRate, PheromoneDeposit, AntsCount, IterationsCount);
}

public record MissionRequest(
    string? Name,
    GeoPointRequest? Start,
    GeoPointRequest? Finish,
    long UavProfileId,
    List<SurveyObjectRequest>? Objects,
    AlgorithmSettingsRequest? Algorithm)
{
    public MissionInputDto ToDto() => new(
        Name ?? string.Empty,
        Start?.ToDto(),
        Finish?.ToDto(),
        UavProfileId,
        Objects?.Select(o => o.ToDto()).ToList() ?? [],
        Algorithm?.ToDto());
}
