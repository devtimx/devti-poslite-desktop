using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DevtiPosLite.Core.Models;

public class User : BaseEntity
{
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(40)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Status { get; set; } = "ACTIVE";

    [MaxLength(50)]
    public string Profile { get; set; } = string.Empty;

    [MaxLength(255)]
    [JsonIgnore]
    public string Password { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Image { get; set; } = string.Empty;

    public uint? RoleId { get; set; }
    public Role? Role { get; set; }
}
