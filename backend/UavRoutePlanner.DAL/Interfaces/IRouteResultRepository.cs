using UavRoutePlanner.DAL.Entities;

namespace UavRoutePlanner.DAL.Interfaces;

public interface IRouteResultRepository : IRepositoriesBase<RouteResultEntity>
{
    Task<List<RouteResultEntity>> GetByMissionIdAsync(long missionId, CancellationToken ct = default);
}
