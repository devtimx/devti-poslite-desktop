using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.Core.Models;
using DevtiPosLite.UI.Dialogs;
using DevtiPosLite.UI.Helpers;

namespace DevtiPosLite.UI.Forms;

public partial class ProductsForm : Form
{
    private readonly ICatalogService _catalogService;
    private List<Product> _products = new();
    private List<Category> _categories = new();

    public ProductsForm(ICatalogService catalogService)
    {
        _catalogService = catalogService;
        InitializeComponent();
    }

    private async void InitializeComponent()
    {
        UITheme.StyleForm(this, "Productos", new Size(1100, 550));

        var toolbar = new ToolStrip();
        toolbar.Items.Add(UITheme.CreateToolbarButton("Nuevo", ButtonStyle.Primary, async (s, e) => await ShowEditDialog(null)));
        toolbar.Items.Add(UITheme.CreateToolbarButton("Editar", ButtonStyle.Secondary, async (s, e) => await ShowEditDialog(GetSelected())));
        toolbar.Items.Add(UITheme.CreateToolbarButton("Eliminar", ButtonStyle.Danger, async (s, e) => await DeleteSelected()));
        toolbar.Items.Add(new ToolStripSeparator());
        toolbar.Items.Add(UITheme.CreateToolbarButton("Actualizar", ButtonStyle.Ghost, async (s, e) => await LoadData()));
        UITheme.StyleToolStrip(toolbar);
        Controls.Add(toolbar);

        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterWidth = 6, SplitterDistance = 680 };

        dgv = new DataGridView
        {
            Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        UITheme.StyleDataGrid(dgv);
        dgv.Columns.Add("Id", "ID");
        dgv.Columns.Add("Barcode", "Código");
        dgv.Columns.Add("Name", "Nombre");
        dgv.Columns.Add("Category", "Categoría");
        dgv.Columns.Add("Price", "Precio");
        dgv.Columns.Add("Cost", "Costo");
        dgv.Columns.Add("Stock", "Stock");
        dgv.Columns.Add("Alerts", "Alerta");
        dgv.Columns["Price"]!.DefaultCellStyle.Format = "N2";
        dgv.Columns["Cost"]!.DefaultCellStyle.Format = "N2";
        split.Panel1.Controls.Add(dgv);

        var previewPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        var lblPreview = new Label { Text = "Vista previa", Font = UITheme.FontBold, Dock = DockStyle.Top, Height = 25, ForeColor = UITheme.TextMuted };
        picPreview = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, BackColor = UITheme.Surface };
        previewPanel.Controls.Add(picPreview);
        previewPanel.Controls.Add(lblPreview);
        split.Panel2.Controls.Add(previewPanel);

        Controls.Add(split);
        split.Resize += (s, e) => UITheme.KeepSplitRatio(split, 0.62);

        dgv.SelectionChanged += (s, e) => ShowPreview();
        dgv.CellDoubleClick += async (s, e) => await ShowEditDialog(GetSelected());

        Load += async (s, e) => await LoadData();
    }

    private DataGridView dgv = null!;
    private PictureBox picPreview = null!;

    private async Task LoadData()
    {
        _products = (await _catalogService.GetProductsAsync()).ToList();
        _categories = (await _catalogService.GetCategoriesAsync()).ToList();
        dgv.Rows.Clear();
        foreach (var p in _products)
        {
            var catName = _categories.FirstOrDefault(c => c.Id == p.CategoryId)?.Name ?? "";
            dgv.Rows.Add(p.Id, p.Barcode, p.Name, catName, p.Price, p.Cost, p.Stock, p.Alerts);
        }
        ShowPreview();
    }

    private void ShowPreview()
    {
        var p = GetSelected();
        picPreview.Image = p != null ? ImageHelper.LoadImage(p.Image) : null;
    }

    private Product? GetSelected()
    {
        if (dgv.CurrentRow == null || dgv.CurrentRow.Index >= _products.Count) return null;
        return _products[dgv.CurrentRow.Index];
    }

    private async Task ShowEditDialog(Product? product)
    {
        _categories = (await _catalogService.GetCategoriesAsync()).ToList();
        if (product == null && _categories.Count == 0)
        {
            MessageBox.Show("Debe crear al menos una categoría antes de agregar productos.\nVaya a Categorías y dé de alta una.", "Sin categorías", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var dialog = new ProductEditDialog(product, _categories);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            if (product == null)
                await _catalogService.CreateProductAsync(dialog.Product);
            else
                await _catalogService.UpdateProductAsync(dialog.Product);
            await LoadData();
        }
    }

    private async Task DeleteSelected()
    {
        var p = GetSelected();
        if (p == null) return;
        if (MessageBox.Show($"¿Eliminar {p.Name}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            ImageHelper.DeleteImage(p.Image);
            await _catalogService.DeleteProductAsync(p.Id);
            await LoadData();
        }
    }
}
