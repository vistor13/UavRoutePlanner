using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Options;
using UavRoutePlanner.DAL.Entities;

namespace UavRoutePlanner.BLL.Services;

public class JwtService(IOptions<AuthOptions> options, TimeProvider timeProvider) : IJwtService
{
    public const string UserIdClaim = "userId";

    private readonly AuthOptions _options = options.Value;

    public string GenerateToken(UserEntity user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            claims: [new Claim(UserIdClaim, user.Id.ToString())],
            notBefore: now,
            expires: now.AddMinutes(_options.ExpiresMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
