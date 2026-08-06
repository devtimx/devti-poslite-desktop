using System.ComponentModel.DataAnnotations;

namespace DevtiPosLite.Core.Models;

public class Return : BaseEntity
{
    public uint SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    public uint ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public uint UserId { get; set; }
    public User User { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal RefundAmount { get; set; }

    [MaxLength(60)]
    public string SaleReference { get; set; } = string.Empty;
}
