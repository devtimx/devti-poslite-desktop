namespace DevtiPosLite.Core.Models;

public class CashOpening : BaseEntity
{
    public uint UserId { get; set; }
    public User User { get; set; } = null!;

    public decimal OpeningAmount { get; set; }
    public decimal ClosingAmount { get; set; }

    public string Status { get; set; } = "OPEN";

    public string Notes { get; set; } = string.Empty;

    public CashoutHistory? CashoutHistory { get; set; }
}
