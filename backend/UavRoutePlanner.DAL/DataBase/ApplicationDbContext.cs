using UavRoutePlanner.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace UavRoutePlanner.DAL.DataBase;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; } = null!;

    public DbSet<UavProfileEntity> UavProfiles { get; set; } = null!;

    public DbSet<UavProfilePhotoEntity> UavProfilePhotos { get; set; } = null!;

    public DbSet<MissionEntity> Missions { get; set; } = null!;

    public DbSet<UavParametersEntity> UavParameters { get; set; } = null!;

    public DbSet<AlgorithmSettingsEntity> AlgorithmSettings { get; set; } = null!;

    public DbSet<SurveyObjectEntity> SurveyObjects { get; set; } = null!;

    public DbSet<RouteResultEntity> RouteResults { get; set; } = null!;

    public DbSet<RouteLegEntity> RouteLegs { get; set; } = null!;

    public DbSet<WeatherForecastEntity> WeatherForecasts { get; set; } = null!;
}
