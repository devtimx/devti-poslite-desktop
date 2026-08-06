using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.Core.Models;
using DevtiPosLite.UI.Dialogs;
using DevtiPosLite.UI.Helpers;

namespace DevtiPosLite.UI.Forms;

public partial class DenominationsForm : Form
{
    private readonly ICatalogService _catalogService;
    private List<Denomination> _denominations = new();

    public DenominationsForm(ICatalogService catalogService)
    {
        _catalogService = catalogService;
        InitializeComponent();
    }

    private async void InitializeComponent()
    {
        UITheme.StyleForm(this, "Denominaciones", new Size(750, 450));

        var toolbar = new ToolStrip();
        toolbar.Items.Add(UITheme.CreateToolbarButton("Nueva", ButtonStyle.Primary, async (s, e) => await NewDenomination()));
        toolbar.Items.Add(UITheme.CreateToolbarButton("Editar", ButtonStyle.Secondary, async (s, e) => await EditDenomination()));
        toolbar.Items.Add(UITheme.CreateToolbarButton("Eliminar", ButtonStyle.Danger, async (s, e) => await DeleteDenomination()));
        toolbar.Items.Add(new ToolStripSeparator());
        toolbar.Items.Add(UITheme.CreateToolbarButton("Actualizar", ButtonStyle.Ghost, async (s, e) => await LoadData()));
        UITheme.StyleToolStrip(toolbar);
        Controls.Add(toolbar);

        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterWidth = 6, SplitterDistance = 450 };

        dgv = new DataGridView
        {
            Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        UITheme.StyleDataGrid(dgv);
        dgv.Columns.Add("Id", "ID");
        dgv.Columns.Add("Type", "Tipo");
        dgv.Columns.Add("Value", "Valor");
        dgv.Columns["Value"]!.DefaultCellStyle.Format = "N2";
        split.Panel1.Controls.Add(dgv);

        var previewPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        var lblPreview = new Label { Text = "Vista previa", Font = UITheme.FontBold, Dock = DockStyle.Top, Height = 25, ForeColor = UITheme.TextMuted };
        picPreview = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, BackColor = UITheme.Surface };
        previewPanel.Controls.Add(picPreview);
        previewPanel.Controls.Add(lblPreview);
        split.Panel2.Controls.Add(previewPanel);

        Controls.Add(split);
        split.Resize += (s, e) => UITheme.KeepSplitRatio(split, 0.6);

        dgv.SelectionChanged += (s, e) => ShowPreview();
        dgv.CellDoubleClick += async (s, e) => await EditDenomination();
        Load += async (s, e) => await LoadData();
    }

    private DataGridView dgv = null!;
    private PictureBox picPreview = null!;

    private async Task LoadData()
    {
        _denominations = (await _catalogService.GetDenominationsAsync()).ToList();
        dgv.Rows.Clear();
        foreach (var d in _denominations)
            dgv.Rows.Add(d.Id, d.Type, d.Value);
        ShowPreview();
    }

    private void ShowPreview()
    {
        var d = GetSelected();
        picPreview.Image = d != null ? ImageHelper.LoadImage(d.Image) : null;
    }

    private Denomination? GetSelected()
    {
        if (dgv.CurrentRow == null || dgv.CurrentRow.Index >= _denominations.Count) return null;
        return _denominations[dgv.CurrentRow.Index];
    }

    private async Task NewDenomination()
    {
        var dialog = new DenominationEditDialog();
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            await _catalogService.CreateDenominationAsync(dialog.Denomination);
            await LoadData();
        }
    }

    private async Task EditDenomination()
    {
        var d = GetSelected();
        if (d == null) return;
        var dialog = new DenominationEditDialog(d);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            await _catalogService.UpdateDenominationAsync(dialog.Denomination);
            await LoadData();
        }
    }

    private async Task DeleteDenomination()
    {
        var d = GetSelected();
        if (d == null) return;
        if (MessageBox.Show($"¿Eliminar denominación ${d.Value}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            ImageHelper.DeleteImage(d.Image);
            await _catalogService.DeleteDenominationAsync(d.Id);
            await LoadData();
        }
    }
}
