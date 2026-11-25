using Bugtrack.Api.Classes;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class TokenService : ITokenService
{
    private readonly JwtSettings settings;

    public TokenService(IOptions<JwtSettings> options)
    {
        settings = options.Value;
    }

    public LoginResponse Create(User user)
    {
        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),
            new Claim(
                "session",
                Guid.NewGuid().ToString()),
            new Claim(
                "role",
                user.SystemRole.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(settings.Key));

        var credentials = new SigningCredentials(
            key,
            settings.Algorithm);

        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(settings.AccessTokenMinutes),
            signingCredentials: credentials);

        return new LoginResponse
        {
            AccessToken = new JwtSecurityTokenHandler()
                .WriteToken(token)
        };
    }
}