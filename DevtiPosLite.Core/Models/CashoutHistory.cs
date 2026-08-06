namespace DevtiPosLite.Core.Models;

public class CashoutHistory : BaseEntity
{
    public uint CashOpeningId { get; set; }
    public CashOpening CashOpening { get; set; } = null!;

    public decimal TotalSales { get; set; }
    public decimal TotalCash { get; set; }
    public decimal DiscrepancyAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
}
