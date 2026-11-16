using UavRoutePlanner.DAL.Entities;

namespace UavRoutePlanner.DAL.Interfaces;

public interface IUavProfilePhotoRepository
{
    Task<UavProfilePhotoEntity?> GetAsync(long profileId, CancellationToken ct = default);

    Task SaveAsync(long profileId, string contentType, byte[] data, DateTime nowUtc, CancellationToken ct = default);

    Task RemoveAsync(long profileId, CancellationToken ct = default);
}
