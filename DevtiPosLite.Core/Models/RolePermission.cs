namespace DevtiPosLite.Core.Models;

public class RolePermission
{
    public uint RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public uint PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
