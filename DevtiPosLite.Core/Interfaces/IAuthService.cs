using DevtiPosLite.Core.DTOs;

namespace DevtiPosLite.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    (uint UserId, string Email, string RoleName)? ValidateToken(string token);
}
