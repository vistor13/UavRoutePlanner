using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UavRoutePlanner.DAL.Entities.Configuration;

public class UavProfileEntityTypeConfiguration : IEntityTypeConfiguration<UavProfileEntity>
{
    public void Configure(EntityTypeBuilder<UavProfileEntity> builder)
    {
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);

        builder.HasOne(p => p.User)
            .WithMany(u => u.UavProfiles)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
