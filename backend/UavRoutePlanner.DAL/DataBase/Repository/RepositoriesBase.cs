using UavRoutePlanner.DAL.Entities.CommonEntity;
using UavRoutePlanner.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace UavRoutePlanner.DAL.DataBase.Repository;

public class RepositoriesBase<TEntity>(ApplicationDbContext context)
    : IRepositoriesBase<TEntity> where TEntity : BaseEntity
{
    protected ApplicationDbContext Context { get; } = context;

    public async Task<TEntity?> GetById(long id, CancellationToken ct = default) =>
        await Context.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<List<TEntity>> GetAll(CancellationToken ct = default) =>
        await Context.Set<TEntity>().AsNoTracking().ToListAsync(ct);

    public async Task Delete(long id, CancellationToken ct = default)
    {
        var entity = await GetById(id, ct);
        if (entity is null) return;

        Context.Set<TEntity>().Remove(entity);
        await Context.SaveChangesAsync(ct);
    }

    public async Task<TEntity> Create(TEntity entity, CancellationToken ct = default)
    {
        await Context.Set<TEntity>().AddAsync(entity, ct);
        await Context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task Update(long id, TEntity entity, CancellationToken ct = default)
    {
        Context.Set<TEntity>().Update(entity);
        await Context.SaveChangesAsync(ct);
    }
}
