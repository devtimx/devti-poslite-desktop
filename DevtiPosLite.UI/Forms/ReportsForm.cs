using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.UI.Helpers;

namespace DevtiPosLite.UI.Forms;

public partial class ReportsForm : Form
{
    private readonly ISalesService _salesService;

    public ReportsForm(ISalesService salesService)
    {
        _salesService = salesService;
        UITheme.StyleForm(this, "Reportes de Ventas", new Size(1050, 550));

        var filterPanel = new Panel { Height = 56, Dock = DockStyle.Top };
        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(8, 10, 8, 0) };
        var lblFrom = new Label { Text = "Desde:", AutoSize = true, Margin = new Padding(0, 6, 0, 0), ForeColor = UITheme.TextMuted };
        dtpFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30), Width = 130, Margin = new Padding(4, 4, 0, 0) };
        var lblTo = new Label { Text = "Hasta:", AutoSize = true, Margin = new Padding(14, 6, 0, 0), ForeColor = UITheme.TextMuted };
        dtpTo = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Width = 130, Margin = new Padding(4, 4, 0, 0) };
        var btnFilter = new Button { Text = "Filtrar", Size = new Size(90, 30), Margin = new Padding(14, 2, 0, 0) };
        UITheme.StyleButton(btnFilter, ButtonStyle.Primary);
        btnFilter.Click += async (s, e) => await LoadReport();

        flow.Controls.AddRange(new Control[] { lblFrom, dtpFrom, lblTo, dtpTo, btnFilter });
        filterPanel.Controls.Add(flow);
        Controls.Add(filterPanel);

        dgv = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        UITheme.StyleDataGrid(dgv);
        dgv.Columns.Add("SaleId", "Folio");
        dgv.Columns.Add("Product", "Producto");
        dgv.Columns.Add("Qty", "Cant");
        dgv.Columns.Add("Price", "Precio");
        dgv.Columns.Add("LineTotal", "Total Línea");
        dgv.Columns.Add("User", "Atendió");
        dgv.Columns.Add("Date", "Fecha");
        Controls.Add(dgv);

        Load += async (s, e) => await LoadReport();
    }

    private DateTimePicker dtpFrom = null!;
    private DateTimePicker dtpTo = null!;
    private DataGridView dgv = null!;

    private async Task LoadReport()
    {
        var lines = await _salesService.GetSalesDetailReportAsync(dtpFrom.Value, dtpTo.Value);
        dgv.Rows.Clear();
        foreach (var l in lines)
            dgv.Rows.Add(l.SaleId, l.ProductName, l.Quantity, l.Price.ToString("N2"), l.LineTotal.ToString("N2"), l.UserName, l.CreatedAt.ToString("g"));
    }
}
