using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Mission;
using UavRoutePlanner.BLL.Services.Common;
using UavRoutePlanner.DAL.Interfaces;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;

public class MissionService(
    IMissionRepository missionRepository,
    IUavProfileRepository profileRepository,
    IMissionValidator validator) : IMissionService
{
    public async Task<List<MissionSummaryDto>> GetAll(long userId, CancellationToken ct = default)
    {
        var missions = await missionRepository.GetByUserIdAsync(userId, ct);
        return missions.Select(m => m.ToSummaryDto()).ToList();
    }

    public async Task<ErrorOr<MissionDto>> GetById(long userId, long id, CancellationToken ct = default)
    {
        var mission = await missionRepository.GetOwnedFullAsync(id, userId, ct);

        return mission is null ? NotFound() : mission.ToDto();
    }

    public async Task<ErrorOr<MissionDto>> Create(
        long userId, MissionInputDto input, CancellationToken ct = default)
    {
        if (validator.Validate(input) is { } error)
            return error;

        var profile = await profileRepository.GetOwnedAsync(input.UavProfileId, userId, ct);
        if (profile is null)
            return Error.NotFound("UavProfileNotFound", Errors.UavProfileNotFound);

        var entity = input.ToEntity(userId, profile, input.Start!, input.Finish!);
        var created = await missionRepository.Create(entity, ct);

        return created.ToDto();
    }

    public async Task<ErrorOr<MissionDto>> Update(
        long userId, long id, MissionInputDto input, CancellationToken ct = default)
    {
        if (validator.Validate(input) is { } error)
            return error;

        var profile = await profileRepository.GetOwnedAsync(input.UavProfileId, userId, ct);
        if (profile is null)
            return Error.NotFound("UavProfileNotFound", Errors.UavProfileNotFound);

        var source = input.ToEntity(userId, profile, input.Start!, input.Finish!);

        if (!await missionRepository.UpdateOwnedAsync(id, userId, source, ct))
            return NotFound();

        var updated = await missionRepository.GetOwnedFullAsync(id, userId, ct);

        return updated is null ? NotFound() : updated.ToDto();
    }

    public async Task<ErrorOr<Deleted>> Delete(long userId, long id, CancellationToken ct = default)
    {
        var mission = await missionRepository.GetOwnedFullAsync(id, userId, ct);
        if (mission is null)
            return NotFound();

        await missionRepository.Delete(id, ct);

        return Result.Deleted;
    }

    private static Error NotFound() =>
        Error.NotFound("MissionNotFound", Errors.MissionNotFound);
}
