using DevtiPosLite.Core.DTOs;
using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.UI.Helpers;
using DevtiPosLite.UI.State;

namespace DevtiPosLite.UI.Forms;

public partial class CashoutForm : Form
{
    private readonly ICashierService _cashierService;
    private readonly AuthStore _authStore;

    public CashoutForm(ICashierService cashierService, AuthStore authStore)
    {
        _cashierService = cashierService;
        _authStore = authStore;
        InitializeComponent();
    }

    private async void InitializeComponent()
    {
        UITheme.StyleForm(this, "Cierre de Caja", new Size(900, 500));

        var toolbar = new ToolStrip();
        toolbar.Items.Add(UITheme.CreateToolbarButton("Realizar Cierre", ButtonStyle.Success, async (s, e) => await DoCloseCash()));
        toolbar.Items.Add(new ToolStripSeparator());

        toolbar.Items.Add(new ToolStripLabel("Desde:"));
        var dtpFrom = new ToolStripControlHost(new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30) });
        toolbar.Items.Add(dtpFrom);
        toolbar.Items.Add(new ToolStripLabel("  Hasta:"));
        var dtpTo = new ToolStripControlHost(new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today });
        toolbar.Items.Add(dtpTo);
        toolbar.Items.Add(UITheme.CreateToolbarButton("Filtrar", ButtonStyle.Primary, async (s, e) => await LoadData(
            ((DateTimePicker)dtpFrom.Control).Value, ((DateTimePicker)dtpTo.Control).Value)));
        toolbar.Items.Add(new ToolStripSeparator());
        toolbar.Items.Add(UITheme.CreateToolbarButton("Actualizar", ButtonStyle.Ghost, async (s, e) => await LoadData()));
        UITheme.StyleToolStrip(toolbar);
        Controls.Add(toolbar);

        dgv = new DataGridView
        {
            Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        UITheme.StyleDataGrid(dgv);
        dgv.Columns.Add("Id", "ID");
        dgv.Columns.Add("TotalSales", "Ventas");
        dgv.Columns["TotalSales"]!.DefaultCellStyle.Format = "N2";
        dgv.Columns.Add("TotalCash", "Efectivo");
        dgv.Columns["TotalCash"]!.DefaultCellStyle.Format = "N2";
        dgv.Columns.Add("Discrepancy", "Diferencia");
        dgv.Columns["Discrepancy"]!.DefaultCellStyle.Format = "N2";
        dgv.Columns.Add("Notes", "Notas");
        dgv.Columns.Add("Date", "Fecha");
        Controls.Add(dgv);

        Load += async (s, e) => await LoadData();
    }

    private DataGridView dgv = null!;

    private async Task LoadData(DateTime? from = null, DateTime? to = null)
    {
        var report = await _cashierService.GetCashoutReportAsync(null, from, to);
        dgv.Rows.Clear();
        foreach (var c in report.Cashouts)
            dgv.Rows.Add(c.Id, c.TotalSales, c.TotalCash, c.DiscrepancyAmount, c.Notes, c.CreatedAt.ToString("g"));
    }

    private async Task DoCloseCash()
    {
        if (!_authStore.UserId.HasValue) return;

        try
        {
            var current = await _cashierService.GetCurrentCashOpeningAsync(_authStore.UserId.Value);
            if (current == null)
            {
                var openNow = MessageBox.Show("No tiene caja abierta. ¿Desea abrir una ahora?",
                    "Caja cerrada", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (openNow == DialogResult.Yes)
                    await OpenCash();
                return;
            }

            var input = new Form();
            UITheme.StyleForm(input, "Cierre de Caja", new Size(350, 200));
            input.StartPosition = FormStartPosition.CenterParent;
            input.FormBorderStyle = FormBorderStyle.FixedDialog;
            input.MaximizeBox = false;
            input.MinimizeBox = false;

            var lbl = new Label { Text = $"Apertura: ${current.OpeningAmount:N2}\nMonto final en caja:", Location = new Point(15, 15), Size = new Size(300, 40), ForeColor = UITheme.TextMuted };
            var nud = new NumericUpDown { Location = new Point(15, 60), Size = new Size(200, 25), DecimalPlaces = 2, Maximum = 999999, Minimum = 0 };
            var txtNotes = new TextBox { Location = new Point(15, 90), Size = new Size(300, 25), PlaceholderText = "Notas (opcional)" };
            var btnOk = new Button { Text = "Cerrar Caja", Location = new Point(60, 125), Size = new Size(110, 34), DialogResult = DialogResult.OK };
            UITheme.StyleButton(btnOk, ButtonStyle.Primary);
            var btnCancel = new Button { Text = "Cancelar", Location = new Point(190, 125), Size = new Size(110, 34), DialogResult = DialogResult.Cancel };
            UITheme.StyleGhostButton(btnCancel);

            input.Controls.Add(lbl); input.Controls.Add(nud); input.Controls.Add(txtNotes);
            input.Controls.Add(btnOk); input.Controls.Add(btnCancel);
            input.AcceptButton = btnOk;

            if (input.ShowDialog() == DialogResult.OK)
            {
                var result = await _cashierService.CloseCashAsync(new CashCloseRequest
                {
                    ClosingAmount = nud.Value,
                    Notes = txtNotes.Text.Trim()
                }, _authStore.UserId.Value);

                MessageBox.Show($"Cierre realizado.\nVentas: ${result.TotalSales:N2}\nEfectivo: ${result.TotalCash:N2}\nDiferencia: ${result.DiscrepancyAmount:N2}",
                    "Cierre exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadData();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task OpenCash()
    {
        var dialog = new Dialogs.CashOpeningDialog();
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            await _cashierService.OpenCashAsync(new CashOpenRequest
            {
                OpeningAmount = dialog.OpeningAmount
            }, _authStore.UserId!.Value);
            MessageBox.Show("Caja abierta correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
