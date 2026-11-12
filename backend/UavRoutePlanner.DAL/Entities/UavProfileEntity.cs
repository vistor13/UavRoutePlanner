using System.ComponentModel.DataAnnotations;
using UavRoutePlanner.DAL.Entities.CommonEntity;
using UavRoutePlanner.DAL.Entities.Configuration;
using Microsoft.EntityFrameworkCore;

namespace UavRoutePlanner.DAL.Entities;

[EntityTypeConfiguration(typeof(UavProfileEntityTypeConfiguration))]
public class UavProfileEntity : BaseEntity
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public double AirspeedMs { get; set; }

    public double MaxFlightTimeMin { get; set; }

    public double ReservePercent { get; set; }

    public double FlightAltitudeM { get; set; }

    public long UserId { get; set; }

    public UserEntity User { get; set; } = null!;
}
