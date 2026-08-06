using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.UI.Helpers;

namespace DevtiPosLite.UI.Forms;

public partial class CashHistoryForm : Form
{
    private readonly ICashierService _cashierService;

    public CashHistoryForm(ICashierService cashierService)
    {
        _cashierService = cashierService;
        UITheme.StyleForm(this, "Historial de Caja", new Size(800, 400));

        dgv = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        UITheme.StyleDataGrid(dgv);
        dgv.Columns.Add("Id", "ID");
        dgv.Columns.Add("Opening", "Apertura");
        dgv.Columns.Add("Closing", "Cierre");
        dgv.Columns.Add("Status", "Estado");
        dgv.Columns.Add("Date", "Fecha");
        Controls.Add(dgv);

        Load += async (s, e) => await LoadData();
    }

    private DataGridView dgv = null!;

    private async Task LoadData()
    {
        var history = await _cashierService.GetCashOpeningHistoryAsync();
        dgv.Rows.Clear();
        foreach (var h in history)
        {
            var closing = h.Status == "OPEN" ? "-" : h.ClosingAmount.ToString("N2");
            dgv.Rows.Add(h.Id, h.OpeningAmount.ToString("N2"), closing, h.Status, h.CreatedAt.ToString("g"));
        }
    }
}
