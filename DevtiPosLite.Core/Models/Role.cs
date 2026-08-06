using System.ComponentModel.DataAnnotations;

namespace DevtiPosLite.Core.Models;

public class Role : BaseEntity
{
    [MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Description { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
