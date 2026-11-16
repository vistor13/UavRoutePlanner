using System.ComponentModel.DataAnnotations;
using UavRoutePlanner.DAL.Entities.CommonEntity;
using UavRoutePlanner.DAL.Entities.Configuration;
using Microsoft.EntityFrameworkCore;

namespace UavRoutePlanner.DAL.Entities;

[EntityTypeConfiguration(typeof(UavProfilePhotoEntityTypeConfiguration))]
public class UavProfilePhotoEntity : BaseEntity
{
    public long UavProfileId { get; set; }

    [Required]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    public byte[] Data { get; set; } = [];
}
