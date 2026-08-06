using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.Core.Models;

namespace DevtiPosLite.Services;

public class AdminService : IAdminService
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Role> _roleRepo;
    private readonly IRepository<Permission> _permissionRepo;
    private readonly IRepository<RolePermission> _rolePermissionRepo;

    public AdminService(
        IRepository<User> userRepo,
        IRepository<Role> roleRepo,
        IRepository<Permission> permissionRepo,
        IRepository<RolePermission> rolePermissionRepo)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _permissionRepo = permissionRepo;
        _rolePermissionRepo = rolePermissionRepo;
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
        => await _userRepo.GetAllAsync();

    public async Task<User> CreateUserAsync(User user, string password)
    {
        user.Password = BCrypt.Net.BCrypt.HashPassword(password);
        var result = await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();
        return result;
    }

    public async Task UpdateUserAsync(User user)
    {
        await _userRepo.UpdateAsync(user);
        await _userRepo.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(uint id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user != null)
        {
            await _userRepo.DeleteAsync(user);
            await _userRepo.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Role>> GetRolesAsync()
        => await _roleRepo.GetAllAsync();

    public async Task<Role> CreateRoleAsync(Role role)
    {
        var result = await _roleRepo.AddAsync(role);
        await _roleRepo.SaveChangesAsync();
        return result;
    }

    public async Task UpdateRoleAsync(Role role)
    {
        await _roleRepo.UpdateAsync(role);
        await _roleRepo.SaveChangesAsync();
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        => await _permissionRepo.GetAllAsync();

    public async Task<IEnumerable<uint>> GetRolePermissionIdsAsync(uint roleId)
    {
        var rps = await _rolePermissionRepo.FindAsync(rp => rp.RoleId == roleId);
        return rps.Select(rp => rp.PermissionId);
    }

    public async Task AssignPermissionsToRoleAsync(uint roleId, IEnumerable<uint> permissionIds)
    {
        var existing = await _rolePermissionRepo.FindAsync(rp => rp.RoleId == roleId);
        foreach (var rp in existing)
            await _rolePermissionRepo.DeleteAsync(rp);

        foreach (var permId in permissionIds)
            await _rolePermissionRepo.AddAsync(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permId
            });

        await _rolePermissionRepo.SaveChangesAsync();
    }
}
