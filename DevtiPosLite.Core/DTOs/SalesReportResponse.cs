using DevtiPosLite.Core.Models;

namespace DevtiPosLite.Core.DTOs;

public class SalesReportResponse
{
    public List<Sale> Sales { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
}

public class SalesReportLineDto
{
    public uint SaleId { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal LineTotal => Price * Quantity;
    public string UserName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class CashoutReportResponse
{
    public List<CashoutHistory> Cashouts { get; set; } = new();
    public decimal TotalSales { get; set; }
    public decimal TotalCash { get; set; }
}
