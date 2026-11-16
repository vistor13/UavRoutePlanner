using Microsoft.EntityFrameworkCore;
using UavRoutePlanner.DAL.Entities;
using UavRoutePlanner.DAL.Interfaces;

namespace UavRoutePlanner.DAL.DataBase.Repository;

public class UavProfilePhotoRepository(ApplicationDbContext context) : IUavProfilePhotoRepository
{
    public async Task<UavProfilePhotoEntity?> GetAsync(long profileId, CancellationToken ct = default) =>
        await context.UavProfilePhotos.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UavProfileId == profileId, ct);

    public async Task SaveAsync(
        long profileId, string contentType, byte[] data, DateTime nowUtc, CancellationToken ct = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct);

        await context.UavProfilePhotos
            .Where(p => p.UavProfileId == profileId)
            .ExecuteDeleteAsync(ct);

        context.UavProfilePhotos.Add(new UavProfilePhotoEntity
        {
            UavProfileId = profileId,
            ContentType = contentType,
            Data = data
        });
        await context.SaveChangesAsync(ct);

        await context.UavProfiles
            .Where(p => p.Id == profileId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.PhotoUpdatedAtUtc, (DateTime?)nowUtc), ct);

        await transaction.CommitAsync(ct);
    }

    public async Task RemoveAsync(long profileId, CancellationToken ct = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct);

        await context.UavProfilePhotos
            .Where(p => p.UavProfileId == profileId)
            .ExecuteDeleteAsync(ct);

        await context.UavProfiles
            .Where(p => p.Id == profileId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.PhotoUpdatedAtUtc, (DateTime?)null), ct);

        await transaction.CommitAsync(ct);
    }
}
