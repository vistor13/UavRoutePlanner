using UavRoutePlanner.DAL.Entities.CommonEntity;
using UavRoutePlanner.DAL.Entities.Configuration;
using Microsoft.EntityFrameworkCore;

namespace UavRoutePlanner.DAL.Entities;

[EntityTypeConfiguration(typeof(UserEntityTypeConfiguration))]
public class UserEntity : BaseEntity
{
    public string UserName { get; set; } = string.Empty;

    public string HashedPassword { get; set; } = string.Empty;

    public ICollection<UavProfileEntity> UavProfiles { get; set; } = new List<UavProfileEntity>();

    public ICollection<MissionEntity> Missions { get; set; } = new List<MissionEntity>();
}
