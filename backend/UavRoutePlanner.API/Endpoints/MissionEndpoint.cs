using System.Security.Claims;
using UavRoutePlanner.Api.Contracts.Request.Mission;
using UavRoutePlanner.Api.Extensions;
using UavRoutePlanner.BLL.Interfaces;

namespace UavRoutePlanner.Api.Endpoints;

public static class MissionEndpoint
{
    public static void MapMissionEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/missions")
            .WithTags("Missions")
            .RequireAuthorization();

        group.MapGet("", GetAll);
        group.MapGet("{id:long}", GetById);
        group.MapPost("", Create);
        group.MapPut("{id:long}", Update);
        group.MapDelete("{id:long}", Delete);
    }

    private static async Task<IResult> GetAll(
        ClaimsPrincipal user, IMissionService service, CancellationToken ct) =>
        Results.Ok(await service.GetAll(user.GetUserId(), ct));

    private static async Task<IResult> GetById(
        long id, ClaimsPrincipal user, IMissionService service, CancellationToken ct) =>
        (await service.GetById(user.GetUserId(), id, ct)).ToResult();

    private static async Task<IResult> Create(
        MissionRequest request, ClaimsPrincipal user, IMissionService service, CancellationToken ct) =>
        (await service.Create(user.GetUserId(), request.ToDto(), ct)).ToResult();

    private static async Task<IResult> Update(
        long id, MissionRequest request, ClaimsPrincipal user, IMissionService service, CancellationToken ct) =>
        (await service.Update(user.GetUserId(), id, request.ToDto(), ct)).ToResult();

    private static async Task<IResult> Delete(
        long id, ClaimsPrincipal user, IMissionService service, CancellationToken ct) =>
        (await service.Delete(user.GetUserId(), id, ct)).ToResult();
}
