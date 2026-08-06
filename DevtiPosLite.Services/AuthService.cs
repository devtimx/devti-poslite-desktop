using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DevtiPosLite.Core.DTOs;
using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.Core.Models;
using DevtiPosLite.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DevtiPosLite.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Name == request.UserName)

            ?? throw new UnauthorizedAccessException("Credenciales inválidas");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            throw new UnauthorizedAccessException("Credenciales inválidas");

        if (user.Status != "ACTIVE")
            throw new UnauthorizedAccessException("Usuario inactivo");

        var token = GenerateJwtToken(user);

        return new AuthResponse { Token = token, User = user };
    }

    public (uint UserId, string Email, string RoleName)? ValidateToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? "DevtiPosLiteSecretKey2024!@#$%^&*()VeryLong"));

            var handler = new JwtSecurityTokenHandler();
            var result = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "DevtiPosLite",
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] ?? "DevtiPosLiteApp",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            var claims = result.Claims;
            return (
                uint.Parse(claims.First(c => c.Type == "userId").Value),
                claims.First(c => c.Type == ClaimTypes.Email).Value,
                claims.First(c => c.Type == "roleName").Value
            );
        }
        catch
        {
            return null;
        }
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "DevtiPosLiteSecretKey2024!@#$%^&*()VeryLong"));

        var claims = new[]
        {
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("roleId", user.RoleId?.ToString() ?? ""),
            new Claim("roleName", user.Role?.Name ?? ""),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "DevtiPosLite",
            audience: _configuration["Jwt:Audience"] ?? "DevtiPosLiteApp",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
