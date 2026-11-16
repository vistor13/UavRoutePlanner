using System.Security.Claims;
using UavRoutePlanner.BLL.Services;

namespace UavRoutePlanner.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static long GetUserId(this ClaimsPrincipal user) =>
        long.Parse(user.FindFirstValue(JwtService.UserIdClaim)
                   ?? throw new InvalidOperationException("The token does not contain a user identifier."));
}
