using System.ComponentModel.DataAnnotations;

namespace DevtiPosLite.Core.Models;

public class Category : BaseEntity
{
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Image { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
