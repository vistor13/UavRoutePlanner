using UavRoutePlanner.BLL.Models.Dto.Route;

namespace UavRoutePlanner.Api.Contracts.Request.Route;

public record DepartureSearchRequest(DateTime? FromUtc, DateTime? ToUtc, int? Seed)
{
    public DepartureSearchDto ToDto() => new(FromUtc, ToUtc, Seed);
}

public record SaveDeparturePlanRequest(string? Name, DateTime DepartureTimeUtc, int Seed)
{
    public SaveDeparturePlanDto ToDto() => new(Name, DepartureTimeUtc, Seed);
}
