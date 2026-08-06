namespace DevtiPosLite.Core.DTOs;

public class CashOpenRequest
{
    public decimal OpeningAmount { get; set; }
    public string? Notes { get; set; }
}

public class CashCloseRequest
{
    public decimal ClosingAmount { get; set; }
    public string? Notes { get; set; }
}
