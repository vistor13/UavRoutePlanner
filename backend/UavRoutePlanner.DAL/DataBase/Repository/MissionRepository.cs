using UavRoutePlanner.DAL.Entities;
using UavRoutePlanner.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace UavRoutePlanner.DAL.DataBase.Repository;

public class MissionRepository(ApplicationDbContext context)
    : RepositoriesBase<MissionEntity>(context), IMissionRepository
{
    public async Task<List<MissionEntity>> GetByUserIdAsync(long userId, CancellationToken ct = default) =>
        await Context.Missions.AsNoTracking()
            .Include(m => m.Objects)
            .Include(m => m.Results)
            .AsSplitQuery()
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

    public async Task<MissionEntity?> GetFullAsync(long id, CancellationToken ct = default) =>
        await FullQuery().FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<MissionEntity?> GetOwnedFullAsync(long id, long userId, CancellationToken ct = default) =>
        await FullQuery().FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId, ct);

    private IQueryable<MissionEntity> FullQuery() =>
        Context.Missions.AsNoTracking()
            .Include(m => m.Uav)
            .Include(m => m.Algorithm)
            .Include(m => m.Objects.OrderBy(o => o.Number))
            .Include(m => m.Results)
                .ThenInclude(r => r.Legs.OrderBy(l => l.Order))
            .AsSplitQuery();
}
