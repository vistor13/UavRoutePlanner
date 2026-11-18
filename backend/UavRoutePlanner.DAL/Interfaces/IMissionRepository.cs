using UavRoutePlanner.DAL.Entities;

namespace UavRoutePlanner.DAL.Interfaces;

public interface IMissionRepository : IRepositoriesBase<MissionEntity>
{
    Task<List<MissionEntity>> GetByUserIdAsync(long userId, CancellationToken ct = default);

    Task<MissionEntity?> GetFullAsync(long id, CancellationToken ct = default);

    Task<MissionEntity?> GetOwnedFullAsync(long id, long userId, CancellationToken ct = default);

    Task<bool> UpdateOwnedAsync(long id, long userId, MissionEntity source, CancellationToken ct = default);
}
