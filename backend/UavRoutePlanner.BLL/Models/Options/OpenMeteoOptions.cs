namespace UavRoutePlanner.BLL.Models.Options;

public class OpenMeteoOptions
{
    public const string SectionName = "OpenMeteo";

    public string BaseUrl { get; set; } = "https://api.open-meteo.com/";

    public int ForecastDays { get; set; } = 3;

  
    public int CacheMinutes { get; set; } = 60;

    public int TimeoutSeconds { get; set; } = 15;
}
