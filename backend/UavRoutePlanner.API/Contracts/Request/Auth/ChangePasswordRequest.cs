using UavRoutePlanner.BLL.Models.Dto.Auth;

namespace UavRoutePlanner.Api.Contracts.Request.Auth;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword)
{
    public ChangePasswordDto ToDto() => new(CurrentPassword, NewPassword);
}
