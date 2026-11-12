using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UavRoutePlanner.DAL.Entities.Configuration;

public class MissionEntityTypeConfiguration : IEntityTypeConfiguration<MissionEntity>
{
    public void Configure(EntityTypeBuilder<MissionEntity> builder)
    {
        builder.Property(m => m.Name).IsRequired().HasMaxLength(200);

        builder.HasOne(m => m.User)
            .WithMany(u => u.Missions)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.UavProfile)
            .WithMany()
            .HasForeignKey(m => m.UavProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.OwnsOne(m => m.Start);
        builder.OwnsOne(m => m.Finish);

        builder.HasOne(m => m.Uav)
            .WithOne(u => u.Mission)
            .HasForeignKey<UavParametersEntity>(u => u.MissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Algorithm)
            .WithOne(a => a.Mission)
            .HasForeignKey<AlgorithmSettingsEntity>(a => a.MissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Objects)
            .WithOne(o => o.Mission)
            .HasForeignKey(o => o.MissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.UserId, m.CreatedAt });
    }
}
