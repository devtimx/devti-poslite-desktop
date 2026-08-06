using System.ComponentModel.DataAnnotations;

namespace DevtiPosLite.Core.Models;

public class Product : BaseEntity
{
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Barcode { get; set; } = string.Empty;

    public decimal Cost { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int Alerts { get; set; }

    [MaxLength(255)]
    public string Image { get; set; } = string.Empty;

    public uint CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
