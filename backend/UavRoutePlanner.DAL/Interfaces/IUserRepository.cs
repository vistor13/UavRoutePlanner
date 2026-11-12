using UavRoutePlanner.DAL.Entities;

namespace UavRoutePlanner.DAL.Interfaces;

public interface IUserRepository : IRepositoriesBase<UserEntity>
{
    Task<UserEntity?> GetByUserName(string userName, CancellationToken ct = default);
}
