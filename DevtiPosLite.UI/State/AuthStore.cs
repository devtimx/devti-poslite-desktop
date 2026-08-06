using DevtiPosLite.Core.Models;

namespace DevtiPosLite.UI.State;

public class AuthStore
{
    private User? _currentUser;
    private string _token = string.Empty;

    public User? CurrentUser
    {
        get => _currentUser;
        set
        {
            _currentUser = value;
            NotifyStateChanged();
        }
    }

    public string Token
    {
        get => _token;
        set
        {
            _token = value;
            NotifyStateChanged();
        }
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_token) && CurrentUser != null;
    public uint? UserId => CurrentUser?.Id;
    public string? RoleName => CurrentUser?.Role?.Name;

    public event Action? OnChange;

    public void SetSession(User user, string token)
    {
        CurrentUser = user;
        Token = token;
    }

    public void Logout()
    {
        CurrentUser = null;
        Token = string.Empty;
        NotifyStateChanged();
    }

    public bool HasPermission(string permissionName)
    {
        return CurrentUser?.Role?.RolePermissions?
            .Any(rp => rp.Permission.Name == permissionName) ?? false;
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
