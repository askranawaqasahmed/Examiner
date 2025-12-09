using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ideageek.Examiner.Api.Options;
using Ideageek.Examiner.Core.Dtos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ideageek.Examiner.Api.Helpers;

public interface IJwtTokenGenerator
{
    AuthResponseDto GenerateToken(UserDto user);
}

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public AuthResponseDto GenerateToken(UserDto user)
    {
        var expires = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.Role, user.Role)
        };

        if (user.StudentId.HasValue)
        {
            claims.Add(new Claim("studentId", user.StudentId.Value.ToString()));
        }

        if (user.TeacherId.HasValue)
        {
            claims.Add(new Claim("teacherId", user.TeacherId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler();

        return new AuthResponseDto
        {
            Token = handler.WriteToken(token),
            ExpiresAt = expires,
            User = user
        };
    }
}
