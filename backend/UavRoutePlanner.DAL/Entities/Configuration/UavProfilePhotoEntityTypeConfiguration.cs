using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UavRoutePlanner.DAL.Entities.Configuration;

public class UavProfilePhotoEntityTypeConfiguration : IEntityTypeConfiguration<UavProfilePhotoEntity>
{
    public void Configure(EntityTypeBuilder<UavProfilePhotoEntity> builder)
    {
        builder.Property(p => p.ContentType).IsRequired().HasMaxLength(50);

        builder.HasIndex(p => p.UavProfileId).IsUnique();

        builder.HasOne<UavProfileEntity>()
            .WithOne()
            .HasForeignKey<UavProfilePhotoEntity>(p => p.UavProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
