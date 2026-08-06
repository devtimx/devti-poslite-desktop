using DevtiPosLite.Core.Models;

namespace DevtiPosLite.Core.DTOs;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}
