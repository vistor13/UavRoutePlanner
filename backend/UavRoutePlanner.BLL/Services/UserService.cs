using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Auth;
using UavRoutePlanner.DAL.Entities;
using UavRoutePlanner.DAL.Interfaces;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;

public class UserService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService) : IUserService
{
    private const int MinUserNameLength = 3;
    private const int MaxUserNameLength = 50;
    private const int MinPasswordLength = 6;

    public async Task<ErrorOr<Created>> Register(RegistrationDto dto, CancellationToken ct = default)
    {
        var userName = dto.UserName.Trim();

        if (userName.Length is < MinUserNameLength or > MaxUserNameLength)
            return Error.Validation("UserNameLength", Errors.UserNameLength);

        if (dto.Password.Length < MinPasswordLength)
            return Error.Validation("PasswordTooShort", Errors.PasswordTooShort);

        if (await userRepository.GetByUserName(userName, ct) is not null)
            return Error.Conflict("UserAlreadyExists", Errors.UserAlreadyExist);

        await userRepository.Create(new UserEntity
        {
            UserName = userName,
            HashedPassword = passwordHasher.Hash(dto.Password)
        }, ct);

        return Result.Created;
    }

    public async Task<ErrorOr<string>> Login(LoginDto dto, CancellationToken ct = default)
    {
        var user = await userRepository.GetByUserName(dto.UserName.Trim(), ct);

        if (user is null || !passwordHasher.Verify(dto.Password, user.HashedPassword))
            return Error.Unauthorized("InvalidCredentials", Errors.InvalidCredentials);

        return jwtService.GenerateToken(user);
    }
}
