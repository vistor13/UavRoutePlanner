using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Auth;
using UavRoutePlanner.BLL.Models.Dto.Auth;
using UavRoutePlanner.BLL.Models.Options;
using Microsoft.Extensions.Options;
using UavRoutePlanner.DAL.Entities;
using UavRoutePlanner.DAL.Interfaces;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;

public class UserService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService,
    ILoginAttemptTracker attemptTracker,
    IOptions<AuthOptions> authOptions) : IUserService
{
    private const int MinUserNameLength = 3;
    private const int MaxUserNameLength = 50;
    private const int MinPasswordLength = 6;

    public async Task<ErrorOr<Created>> Register(RegistrationDto dto, CancellationToken ct = default)
    {
        var userName = Normalize(dto.UserName);

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
        var userName = Normalize(dto.UserName);

        if (attemptTracker.IsLocked(userName))
            return AuthErrors.TooManyAttempts(authOptions.Value.LockoutMinutes);

        var user = await userRepository.GetByUserName(userName, ct);

        if (user is null || !passwordHasher.Verify(dto.Password, user.HashedPassword))
        {
            attemptTracker.RegisterFailure(userName);
            return Error.Unauthorized("InvalidCredentials", Errors.InvalidCredentials);
        }

        attemptTracker.Reset(userName);

        return jwtService.GenerateToken(user);
    }

    private static string Normalize(string userName) => userName.Trim().ToLowerInvariant();
}
