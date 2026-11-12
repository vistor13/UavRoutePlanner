using ErrorOr;
using UavRoutePlanner.BLL.Models.Dto.Route;
using UavRoutePlanner.BLL.Models.Weather;
using UavRoutePlanner.DAL.Entities;

namespace UavRoutePlanner.BLL.Interfaces;


public interface IWindResolver
{
    Task<ErrorOr<Wind>> Resolve(MissionEntity mission, RouteRequestDto request, DateTime departureHourUtc, CancellationToken ct = default);
}
