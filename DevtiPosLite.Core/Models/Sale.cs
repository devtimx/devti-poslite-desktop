namespace DevtiPosLite.Core.Models;

public class Sale : BaseEntity
{
    public decimal Total { get; set; }
    public int Items { get; set; }
    public decimal Cash { get; set; }
    public decimal Change { get; set; }

    public string Status { get; set; } = "PAID";

    public uint UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<SaleDetail> Details { get; set; } = new List<SaleDetail>();
}
