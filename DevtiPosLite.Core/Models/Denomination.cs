using System.ComponentModel.DataAnnotations;

namespace DevtiPosLite.Core.Models;

public class Denomination : BaseEntity
{
    [MaxLength(20)]
    public string Type { get; set; } = string.Empty;

    public decimal Value { get; set; }

    [MaxLength(255)]
    public string Image { get; set; } = string.Empty;
}
