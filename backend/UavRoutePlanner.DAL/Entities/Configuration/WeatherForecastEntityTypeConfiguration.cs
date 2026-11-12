using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UavRoutePlanner.DAL.Entities.Configuration;

public class WeatherForecastEntityTypeConfiguration : IEntityTypeConfiguration<WeatherForecastEntity>
{
    public void Configure(EntityTypeBuilder<WeatherForecastEntity> builder)
    {
        builder.HasIndex(w => new { w.Latitude, w.Longitude, w.HeightM, w.ForecastTimeUtc })
            .IsUnique();
    }
}
