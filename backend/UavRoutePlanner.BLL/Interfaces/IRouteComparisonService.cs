using ErrorOr;
using UavRoutePlanner.BLL.Models.Dto.Route;

namespace UavRoutePlanner.BLL.Interfaces;

public interface IRouteComparisonService
{
    
    Task<ErrorOr<RouteComparisonDto>> Compare(long userId, long missionId, RouteRequestDto request, CancellationToken ct = default);
}
