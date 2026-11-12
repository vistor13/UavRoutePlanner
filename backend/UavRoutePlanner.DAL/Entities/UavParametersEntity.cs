using UavRoutePlanner.DAL.Entities.CommonEntity;

namespace UavRoutePlanner.DAL.Entities;


public class UavParametersEntity : BaseEntity
{
    public double AirspeedMs { get; set; }

    public double MaxFlightTimeMin { get; set; }

    public double ReservePercent { get; set; }

    public double FlightAltitudeM { get; set; }

    public long MissionId { get; set; }
    public MissionEntity Mission { get; set; } = null!;
}
