using UavRoutePlanner.DAL.Entities.CommonEntity;

namespace UavRoutePlanner.DAL.Interfaces;

public interface IRepositoriesBase<TEntity> where TEntity : BaseEntity
{
    Task<List<TEntity>> GetAll(CancellationToken ct = default);
    Task<TEntity?> GetById(long id, CancellationToken ct = default);
    Task Delete(long id, CancellationToken ct = default);
    Task<TEntity> Create(TEntity entity, CancellationToken ct = default);
    Task Update(long id, TEntity entity, CancellationToken ct = default);
}
