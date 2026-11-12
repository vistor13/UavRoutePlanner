using UavRoutePlanner.DAL.Entities;
using UavRoutePlanner.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace UavRoutePlanner.DAL.DataBase.Repository;

public class UavProfileRepository(ApplicationDbContext context)
    : RepositoriesBase<UavProfileEntity>(context), IUavProfileRepository
{
    public async Task<List<UavProfileEntity>> GetByUserIdAsync(long userId, CancellationToken ct = default) =>
        await Context.UavProfiles.AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

    public async Task<UavProfileEntity?> GetOwnedAsync(long id, long userId, CancellationToken ct = default) =>
        await Context.UavProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, ct);
}
