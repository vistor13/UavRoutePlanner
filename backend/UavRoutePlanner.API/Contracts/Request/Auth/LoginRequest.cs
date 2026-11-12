using UavRoutePlanner.BLL.Models.Dto.Auth;

namespace UavRoutePlanner.Api.Contracts.Request.Auth;

public record LoginRequest(string UserName, string Password)
{
    public LoginDto ToDto() => new(UserName, Password);
}
