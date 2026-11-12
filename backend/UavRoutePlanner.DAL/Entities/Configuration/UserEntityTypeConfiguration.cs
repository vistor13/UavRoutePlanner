using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UavRoutePlanner.DAL.Entities.Configuration;

public class UserEntityTypeConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.Property(u => u.UserName).IsRequired().HasMaxLength(100);
        builder.HasIndex(u => u.UserName).IsUnique();
    }
}
