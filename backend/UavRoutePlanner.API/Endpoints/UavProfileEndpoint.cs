using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using UavRoutePlanner.Api.Contracts.Request.UavProfile;
using UavRoutePlanner.Api.Extensions;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Services;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

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
        group.MapGet("{id:long}/photo", GetPhoto);
        group.MapPut("{id:long}/photo", SavePhoto);
        group.MapDelete("{id:long}/photo", RemovePhoto);
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

    private static async Task<IResult> GetPhoto(
        long id, ClaimsPrincipal user, IUavProfilePhotoService service, CancellationToken ct) =>
        (await service.Get(user.GetUserId(), id, ct)).ToPhotoResult();

    private static async Task<IResult> SavePhoto(
        long id, HttpContext http, ClaimsPrincipal user, IUavProfilePhotoService service, CancellationToken ct)
    {
        var limit = http.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (limit is { IsReadOnly: false })
            limit.MaxRequestBodySize = UavProfilePhotoService.MaxBytes + 1;

        using var buffer = new MemoryStream();

        try
        {
            await http.Request.Body.CopyToAsync(buffer, ct);
        }
        catch (BadHttpRequestException)
        {
            return Results.Problem(statusCode: StatusCodes.Status413PayloadTooLarge, title: Errors.PhotoTooLarge);
        }

        return (await service.Save(user.GetUserId(), id, buffer.ToArray(), ct)).ToResult();
    }

    private static async Task<IResult> RemovePhoto(
        long id, ClaimsPrincipal user, IUavProfilePhotoService service, CancellationToken ct) =>
        (await service.Remove(user.GetUserId(), id, ct)).ToResult();
}
