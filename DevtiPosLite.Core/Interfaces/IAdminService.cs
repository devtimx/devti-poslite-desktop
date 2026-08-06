using DevtiPosLite.Core.Models;

namespace DevtiPosLite.Core.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<User>> GetUsersAsync();
    Task<User> CreateUserAsync(User user, string password);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(uint id);

    Task<IEnumerable<Role>> GetRolesAsync();
    Task<Role> CreateRoleAsync(Role role);
    Task UpdateRoleAsync(Role role);

    Task<IEnumerable<Permission>> GetPermissionsAsync();
    Task<IEnumerable<uint>> GetRolePermissionIdsAsync(uint roleId);
    Task AssignPermissionsToRoleAsync(uint roleId, IEnumerable<uint> permissionIds);
}
