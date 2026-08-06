namespace DevtiPosLite.Core.DTOs;

public class SaleRequest
{
    public List<SaleItemRequest> Items { get; set; } = new();
    public decimal Cash { get; set; }
}

public class SaleItemRequest
{
    public uint ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
