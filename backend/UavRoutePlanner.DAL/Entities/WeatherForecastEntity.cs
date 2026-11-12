using UavRoutePlanner.DAL.Entities.CommonEntity;
using UavRoutePlanner.DAL.Entities.Configuration;
using Microsoft.EntityFrameworkCore;

namespace UavRoutePlanner.DAL.Entities;

[EntityTypeConfiguration(typeof(WeatherForecastEntityTypeConfiguration))]
public class WeatherForecastEntity : BaseEntity
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public DateTime ForecastTimeUtc { get; set; }

    public int HeightM { get; set; }

    public double WindSpeedMs { get; set; }
    public double WindDirectionFromDeg { get; set; }   
    public DateTime FetchedAtUtc { get; set; } = DateTime.UtcNow;
}
