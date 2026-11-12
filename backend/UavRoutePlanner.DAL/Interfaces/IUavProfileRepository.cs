using UavRoutePlanner.DAL.Entities;

namespace UavRoutePlanner.DAL.Interfaces;

public interface IUavProfileRepository : IRepositoriesBase<UavProfileEntity>
{
    Task<List<UavProfileEntity>> GetByUserIdAsync(long userId, CancellationToken ct = default);

    Task<UavProfileEntity?> GetOwnedAsync(long id, long userId, CancellationToken ct = default);
}
