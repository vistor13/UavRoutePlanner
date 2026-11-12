using UavRoutePlanner.DAL.Entities;
using UavRoutePlanner.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace UavRoutePlanner.DAL.DataBase.Repository;

public class UserRepository(ApplicationDbContext context) : RepositoriesBase<UserEntity>(context), IUserRepository
{
    public async Task<UserEntity?> GetByUserName(string userName, CancellationToken ct = default) =>
        await Context.Users.AsNoTracking().FirstOrDefaultAsync(user => user.UserName == userName, ct);
}
