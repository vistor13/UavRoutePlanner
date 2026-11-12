using UavRoutePlanner.BLL.Models.Dto.UavProfile;
using UavRoutePlanner.DAL.Entities;

namespace UavRoutePlanner.BLL.Services.Common;

public static class UavProfileMapping
{
    public static UavProfileDto ToDto(this UavProfileEntity entity) => new(
        entity.Id,
        entity.Name,
        entity.AirspeedMs,
        entity.MaxFlightTimeMin,
        entity.ReservePercent,
        entity.FlightAltitudeM);

    public static UavProfileEntity ToEntity(this UavProfileInputDto input, long userId) => new()
    {
        Name = input.Name.Trim(),
        AirspeedMs = input.AirspeedMs,
        MaxFlightTimeMin = input.MaxFlightTimeMin,
        ReservePercent = input.ReservePercent,
        FlightAltitudeM = input.FlightAltitudeM,
        UserId = userId
    };
}
