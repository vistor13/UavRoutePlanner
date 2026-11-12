using UavRoutePlanner.BLL.Models.Dto.Auth;

namespace UavRoutePlanner.Api.Contracts.Request.Auth;

public record RegistrationRequest(string UserName, string Password)
{
    public RegistrationDto ToDto() => new(UserName, Password);
}
