using UavRoutePlanner.BLL.Models.Dto.Route;

namespace UavRoutePlanner.Api.Contracts.Request.Route;

public record DepartureSearchRequest(int? Hours, int? Seed)
{
    public DepartureSearchDto ToDto() => new(Hours, Seed);
}
