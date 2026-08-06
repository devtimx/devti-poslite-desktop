using System.ComponentModel.DataAnnotations;

namespace DevtiPosLite.Core.Models;

public class Permission : BaseEntity
{
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
