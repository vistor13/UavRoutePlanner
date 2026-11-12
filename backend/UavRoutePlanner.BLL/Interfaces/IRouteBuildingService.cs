using ErrorOr;
using UavRoutePlanner.BLL.Models.Dto.Route;

namespace UavRoutePlanner.BLL.Interfaces;

public interface IRouteBuildingService
{
    
    Task<ErrorOr<RouteResultDto>> Build(long userId, long missionId, RouteRequestDto request, CancellationToken ct = default);
}
