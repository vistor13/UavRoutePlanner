using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.UavProfile;
using UavRoutePlanner.BLL.Services.Common;
using UavRoutePlanner.DAL.Interfaces;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;

public class UavProfilePhotoService(
    IUavProfileRepository profileRepository,
    IUavProfilePhotoRepository photoRepository,
    TimeProvider timeProvider) : IUavProfilePhotoService
{
    public const int MaxBytes = 2 * 1024 * 1024;

    private const string Jpeg = "image/jpeg";
    private const string Png = "image/png";
    private const string WebP = "image/webp";

    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public async Task<ErrorOr<UavProfilePhotoDto>> Get(long userId, long profileId, CancellationToken ct = default)
    {
        if (await profileRepository.GetOwnedAsync(profileId, userId, ct) is null)
            return ProfileNotFound();

        var photo = await photoRepository.GetAsync(profileId, ct);

        return photo is null
            ? Error.NotFound("UavProfilePhotoNotFound", Errors.PhotoNotFound)
            : new UavProfilePhotoDto(photo.Data, photo.ContentType);
    }

    public async Task<ErrorOr<UavProfileDto>> Save(
        long userId, long profileId, byte[] data, CancellationToken ct = default)
    {
        if (await profileRepository.GetOwnedAsync(profileId, userId, ct) is null)
            return ProfileNotFound();

        if (data.Length > MaxBytes)
            return Error.Validation("PhotoTooLarge", Errors.PhotoTooLarge);

        if (DetectContentType(data) is not { } contentType)
            return Error.Validation("PhotoUnsupportedFormat", Errors.PhotoUnsupportedFormat);

        await photoRepository.SaveAsync(profileId, contentType, data, timeProvider.GetUtcNow().UtcDateTime, ct);

        return await Reload(userId, profileId, ct);
    }

    public async Task<ErrorOr<UavProfileDto>> Remove(long userId, long profileId, CancellationToken ct = default)
    {
        if (await profileRepository.GetOwnedAsync(profileId, userId, ct) is null)
            return ProfileNotFound();

        await photoRepository.RemoveAsync(profileId, ct);

        return await Reload(userId, profileId, ct);
    }

    private async Task<ErrorOr<UavProfileDto>> Reload(long userId, long profileId, CancellationToken ct)
    {
        var profile = await profileRepository.GetOwnedAsync(profileId, userId, ct);

        return profile is null ? ProfileNotFound() : profile.ToDto();
    }

    private static string? DetectContentType(byte[] data)
    {
        if (data.AsSpan().StartsWith(JpegSignature))
            return Jpeg;

        if (data.AsSpan().StartsWith(PngSignature))
            return Png;

        if (data.Length >= 12
            && data.AsSpan(0, 4).SequenceEqual("RIFF"u8)
            && data.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            return WebP;

        return null;
    }

    private static Error ProfileNotFound() =>
        Error.NotFound("UavProfileNotFound", Errors.UavProfileNotFound);
}
