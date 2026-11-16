using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.UavProfile;
using UavRoutePlanner.BLL.Services.Common;
using UavRoutePlanner.DAL.Interfaces;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;

public class UavProfileService(IUavProfileRepository profileRepository) : IUavProfileService
{
    private const int MaxNameLength = 100;

    private const double MaxAltitudeM = 180;

    private const double MaxAirspeedMs = 100;

    private const double MaxFlightTimeMin = 720;

    public async Task<List<UavProfileDto>> GetAll(long userId, CancellationToken ct = default)
    {
        var profiles = await profileRepository.GetByUserIdAsync(userId, ct);
        return profiles.Select(p => p.ToDto()).ToList();
    }

    public async Task<ErrorOr<UavProfileDto>> GetById(long userId, long id, CancellationToken ct = default)
    {
        var profile = await profileRepository.GetOwnedAsync(id, userId, ct);

        return profile is null ? NotFound() : profile.ToDto();
    }

    public async Task<ErrorOr<UavProfileDto>> Create(
        long userId, UavProfileInputDto input, CancellationToken ct = default)
    {
        if (Validate(input) is { } error)
            return error;

        var created = await profileRepository.Create(input.ToEntity(userId), ct);

        return created.ToDto();
    }

    public async Task<ErrorOr<UavProfileDto>> Update(
        long userId, long id, UavProfileInputDto input, CancellationToken ct = default)
    {
        if (Validate(input) is { } error)
            return error;

        var existing = await profileRepository.GetOwnedAsync(id, userId, ct);
        if (existing is null)
            return NotFound();

        var entity = input.ToEntity(userId);
        entity.Id = id;
        entity.PhotoUpdatedAtUtc = existing.PhotoUpdatedAtUtc;
        await profileRepository.Update(id, entity, ct);

        return entity.ToDto();
    }

    public async Task<ErrorOr<Deleted>> Delete(long userId, long id, CancellationToken ct = default)
    {
        if (await profileRepository.GetOwnedAsync(id, userId, ct) is null)
            return NotFound();

        await profileRepository.Delete(id, ct);

        return Result.Deleted;
    }

    private static Error NotFound() =>
        Error.NotFound("UavProfileNotFound", Errors.UavProfileNotFound);

    private static Error? Validate(UavProfileInputDto input)
    {
        var name = input.Name.Trim();

        if (name.Length is 0 or > MaxNameLength)
            return Error.Validation("UavProfileName", Errors.UavProfileName);

        if (input.AirspeedMs <= 0)
            return Error.Validation("AirspeedNotPositive", Errors.AirspeedNotPositive);

        if (input.AirspeedMs > MaxAirspeedMs)
            return Error.Validation("AirspeedTooHigh", Errors.AirspeedTooHigh);

        if (input.MaxFlightTimeMin <= 0)
            return Error.Validation("MaxFlightTimeNotPositive", Errors.MaxFlightTimeNotPositive);

        if (input.MaxFlightTimeMin > MaxFlightTimeMin)
            return Error.Validation("MaxFlightTimeTooLong", Errors.MaxFlightTimeTooLong);

        if (input.ReservePercent is < 0 or >= 100)
            return Error.Validation("ReserveOutOfRange", Errors.ReserveOutOfRange);

        if (input.FlightAltitudeM is <= 0 or > MaxAltitudeM)
            return Error.Validation("AltitudeOutOfRange", Errors.AltitudeOutOfRange);

        return null;
    }
}
