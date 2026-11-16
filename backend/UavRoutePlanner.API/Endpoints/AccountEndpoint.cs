using System.Security.Claims;
using UavRoutePlanner.Api.Contracts.Request.Auth;
using UavRoutePlanner.Api.Extensions;
using UavRoutePlanner.BLL.Interfaces;

namespace UavRoutePlanner.Api.Endpoints;

public static class AccountEndpoint
{
    public static void MapAccountEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/account")
            .WithTags("Account")
            .RequireAuthorization();

        group.MapGet("me", GetProfile);
        group.MapPut("password", ChangePassword)
            .RequireRateLimiting(RateLimitingExtensions.AuthPolicy);
    }

    private static async Task<IResult> GetProfile(
        ClaimsPrincipal user,
        IUserService userService,
        CancellationToken ct) =>
        (await userService.GetProfile(user.GetUserId(), ct)).ToResult();

    private static async Task<IResult> ChangePassword(
        ChangePasswordRequest request,
        ClaimsPrincipal user,
        IUserService userService,
        CancellationToken ct) =>
        (await userService.ChangePassword(user.GetUserId(), request.ToDto(), ct)).ToResult();
}
