namespace DevtiPosLite.Core.Models;

public class SaleDetail : BaseEntity
{
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public uint ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public uint SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
}
