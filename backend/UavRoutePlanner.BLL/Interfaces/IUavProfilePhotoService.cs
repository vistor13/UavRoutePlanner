using ErrorOr;
using UavRoutePlanner.BLL.Models.Dto.UavProfile;

namespace UavRoutePlanner.BLL.Interfaces;

public interface IUavProfilePhotoService
{
    Task<ErrorOr<UavProfilePhotoDto>> Get(long userId, long profileId, CancellationToken ct = default);

    Task<ErrorOr<UavProfileDto>> Save(long userId, long profileId, byte[] data, CancellationToken ct = default);

    Task<ErrorOr<UavProfileDto>> Remove(long userId, long profileId, CancellationToken ct = default);
}
