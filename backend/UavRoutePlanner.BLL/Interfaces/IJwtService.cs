using UavRoutePlanner.DAL.Entities;

namespace UavRoutePlanner.BLL.Interfaces;

public interface IJwtService
{
    string GenerateToken(UserEntity user);
}
