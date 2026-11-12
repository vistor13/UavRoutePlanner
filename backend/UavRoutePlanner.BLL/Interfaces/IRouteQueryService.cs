using ErrorOr;
using UavRoutePlanner.BLL.Models.Dto.Route;

namespace UavRoutePlanner.BLL.Interfaces;


public interface IRouteQueryService
{
    Task<ErrorOr<List<RouteResultDto>>> GetByMission(long userId, long missionId, CancellationToken ct = default);

    Task<ErrorOr<RouteResultDto>> GetById(long userId, long missionId, long resultId, CancellationToken ct = default);
}
