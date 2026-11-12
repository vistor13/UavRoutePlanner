using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UavRoutePlanner.DAL.Entities.Configuration;

public class RouteResultEntityTypeConfiguration : IEntityTypeConfiguration<RouteResultEntity>
{
    public void Configure(EntityTypeBuilder<RouteResultEntity> builder)
    {
        builder.Property(r => r.WindMode).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(r => r.Mission)
            .WithMany(m => m.Results)
            .HasForeignKey(r => r.MissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Legs)
            .WithOne(l => l.RouteResult)
            .HasForeignKey(l => l.RouteResultId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
