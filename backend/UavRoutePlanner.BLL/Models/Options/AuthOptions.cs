namespace UavRoutePlanner.BLL.Models.Options;

public class AuthOptions
{
    public const string SectionName = "Jwt";
    public const int MinSecretKeyLength = 32;

    public string SecretKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public int ExpiresMinutes { get; set; } = 60 * 24;
}
