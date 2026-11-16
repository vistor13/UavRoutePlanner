using ErrorOr;
using UavRoutePlanner.BLL.Models.Dto.Auth;

namespace UavRoutePlanner.BLL.Interfaces;

public interface IUserService
{
    Task<ErrorOr<Created>> Register(RegistrationDto dto, CancellationToken ct = default);

    Task<ErrorOr<string>> Login(LoginDto dto, CancellationToken ct = default);

    Task<ErrorOr<UserProfileDto>> GetProfile(long userId, CancellationToken ct = default);

    Task<ErrorOr<Updated>> ChangePassword(long userId, ChangePasswordDto dto, CancellationToken ct = default);
}
