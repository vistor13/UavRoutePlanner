using ErrorOr;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Models.Auth;

public static class AuthErrors
{
    public const int TooManyAttemptsType = 429;

    public static Error TooManyAttempts(int lockoutMinutes) =>
        Error.Custom(TooManyAttemptsType, "TooManyLoginAttempts", string.Format(Errors.TooManyLoginAttempts, lockoutMinutes));
}
