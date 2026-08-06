namespace DevtiPosLite.Core.Models;

public class StoreConfig : BaseEntity
{
    public string StoreName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string RFC { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string LogoPath { get; set; } = string.Empty;
    public decimal IVARate { get; set; } = 0.16m;
    public string TicketHeader { get; set; } = string.Empty;
    public string TicketFooter { get; set; } = string.Empty;
    public bool PrintTicket { get; set; } = true;
    public bool ShowIVABreakdown { get; set; } = true;
    public string DefaultPrinter { get; set; } = string.Empty;
    public bool AutoPrint { get; set; }
    public int PrintCopies { get; set; } = 1;
}
