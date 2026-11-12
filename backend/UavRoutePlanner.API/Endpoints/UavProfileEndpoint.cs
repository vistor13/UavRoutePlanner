using System.Security.Claims;
using UavRoutePlanner.Api.Contracts.Request.UavProfile;
using UavRoutePlanner.Api.Extensions;
using UavRoutePlanner.BLL.Interfaces;

namespace UavRoutePlanner.Api.Endpoints;

public static class UavProfileEndpoint
{
    public static void MapUavProfileEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/uav-profiles")
            .WithTags("UavProfiles")
            .RequireAuthorization();

        group.MapGet("", GetAll);
        group.MapGet("{id:long}", GetById);
        group.MapPost("", Create);
        group.MapPut("{id:long}", Update);
        group.MapDelete("{id:long}", Delete);
    }

    private static async Task<IResult> GetAll(
        ClaimsPrincipal user, IUavProfileService service, CancellationToken ct) =>
        Results.Ok(await service.GetAll(user.GetUserId(), ct));

    private static async Task<IResult> GetById(
        long id, ClaimsPrincipal user, IUavProfileService service, CancellationToken ct) =>
        (await service.GetById(user.GetUserId(), id, ct)).ToResult();

    private static async Task<IResult> Create(
        UavProfileRequest request, ClaimsPrincipal user, IUavProfileService service, CancellationToken ct) =>
        (await service.Create(user.GetUserId(), request.ToDto(), ct)).ToResult();

    private static async Task<IResult> Update(
        long id, UavProfileRequest request, ClaimsPrincipal user, IUavProfileService service, CancellationToken ct) =>
        (await service.Update(user.GetUserId(), id, request.ToDto(), ct)).ToResult();

    private static async Task<IResult> Delete(
        long id, ClaimsPrincipal user, IUavProfileService service, CancellationToken ct) =>
        (await service.Delete(user.GetUserId(), id, ct)).ToResult();
}
