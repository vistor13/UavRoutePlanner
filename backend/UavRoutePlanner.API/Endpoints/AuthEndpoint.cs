using UavRoutePlanner.Api.Contracts.Request.Auth;
using UavRoutePlanner.Api.Contracts.Response.Auth;
using UavRoutePlanner.Api.Extensions;
using UavRoutePlanner.BLL.Interfaces;

namespace UavRoutePlanner.Api.Endpoints;

public static class AuthEndpoint
{
    public static void MapAuthEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/auth")
            .WithTags("Auth")
            .RequireRateLimiting(RateLimitingExtensions.AuthPolicy);

        group.MapPost("register", Register);
        group.MapPost("login", Login);
    }

    private static async Task<IResult> Register(
        RegistrationRequest request,
        IUserService userService,
        CancellationToken ct)
    {
        var result = await userService.Register(request.ToDto(), ct);
        return result.ToResult();
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        IUserService userService,
        CancellationToken ct)
    {
        var result = await userService.Login(request.ToDto(), ct);
        return result.Then(token => new TokenResponse(token)).ToResult();
    }
}
