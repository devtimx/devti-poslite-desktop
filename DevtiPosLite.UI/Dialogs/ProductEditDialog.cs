using DevtiPosLite.Core.Models;
using DevtiPosLite.UI.Helpers;

namespace DevtiPosLite.UI.Dialogs;

public partial class ProductEditDialog : Form
{
    public Product Product { get; private set; } = new();
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public List<Category> Categories { get; set; } = new();
    private string? _selectedImagePath;

    public ProductEditDialog(Product? product = null, List<Category>? categories = null)
    {
        if (product != null) Product = product;
        if (categories != null) Categories = categories;
        InitializeComponent();
        if (product != null) LoadProduct(product);
    }

    private void LoadProduct(Product p)
    {
        txtName.Text = p.Name;
        txtBarcode.Text = p.Barcode;
        nudCost.Value = p.Cost;
        nudPrice.Value = p.Price;
        nudStock.Value = p.Stock;
        nudAlerts.Value = p.Alerts;
        if (!string.IsNullOrEmpty(p.Image))
            ShowPreview(p.Image);
        if (cmbCategory.Items.Count > 0)
        {
            var idx = Categories.FindIndex(c => c.Id == p.CategoryId);
            if (idx >= 0) cmbCategory.SelectedIndex = idx;
        }
    }

    private void InitializeComponent()
    {
        UITheme.StyleForm(this, "Producto", new Size(520, 480));
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var mainTbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(10) };
        mainTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        mainTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

        var leftTbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8 };
        leftTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        leftTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

        AddRow(leftTbl, "Nombre:", txtName = new TextBox(), 0);
        AddRow(leftTbl, "Código:", txtBarcode = new TextBox(), 1);
        AddRow(leftTbl, "Costo:", nudCost = new NumericUpDown { DecimalPlaces = 2, Maximum = 999999 }, 2);
        AddRow(leftTbl, "Precio:", nudPrice = new NumericUpDown { DecimalPlaces = 2, Maximum = 999999 }, 3);
        AddRow(leftTbl, "Stock:", nudStock = new NumericUpDown { Maximum = 999999 }, 4);
        AddRow(leftTbl, "Alerta stock:", nudAlerts = new NumericUpDown { Maximum = 999999 }, 5);

        leftTbl.Controls.Add(new Label { Text = "Categoría:", TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Right }, 0, 6);
        cmbCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
        cmbCategory.Items.AddRange(Categories.Select(c => c.Name).ToArray());
        leftTbl.Controls.Add(cmbCategory, 1, 6);

        leftTbl.Controls.Add(new Label { Text = "Imagen:", TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Right }, 0, 7);
        var imgPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        txtImage = new TextBox { ReadOnly = true, Width = 120 };
        var btnBrowse = new Button { Text = "Examinar", AutoSize = true };
        UITheme.StyleGhostButton(btnBrowse);
        btnBrowse.Click += BrowseImage;
        imgPanel.Controls.Add(txtImage);
        imgPanel.Controls.Add(btnBrowse);
        leftTbl.Controls.Add(imgPanel, 1, 7);

        var rightPanel = new Panel { Dock = DockStyle.Fill };
        picPreview = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, BackColor = UITheme.Surface };
        rightPanel.Controls.Add(picPreview);

        mainTbl.Controls.Add(leftTbl, 0, 0);
        mainTbl.Controls.Add(rightPanel, 1, 0);

        var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, FlowDirection = FlowDirection.RightToLeft };
        var btnOk = new Button { Text = "Guardar", DialogResult = DialogResult.OK, Width = 110, Height = 34 };
        UITheme.StyleButton(btnOk, ButtonStyle.Primary);
        btnOk.Click += (s, e) => SaveProduct();
        var btnCancel = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Width = 110, Height = 34 };
        UITheme.StyleGhostButton(btnCancel);
        btnPanel.Controls.Add(btnCancel);
        btnPanel.Controls.Add(btnOk);

        Controls.Add(mainTbl);
        Controls.Add(btnPanel);
        AcceptButton = btnOk;
        Load += (s, e) => { if (cmbCategory.Items.Count > 0) cmbCategory.SelectedIndex = 0; };
    }

    private TextBox txtName = null!, txtBarcode = null!, txtImage = null!;
    private NumericUpDown nudCost = null!, nudPrice = null!, nudStock = null!, nudAlerts = null!;
    private ComboBox cmbCategory = null!;
    private PictureBox picPreview = null!;

    private void AddRow(TableLayoutPanel tbl, string label, Control ctrl, int row)
    {
        tbl.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Right }, 0, row);
        ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        tbl.Controls.Add(ctrl, 1, row);
    }

    private void BrowseImage(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog();
        ofd.Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
        ofd.Title = "Seleccionar imagen";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            _selectedImagePath = ofd.FileName;
            txtImage.Text = ofd.FileName;
            try { picPreview.Image = Image.FromFile(ofd.FileName); }
            catch { picPreview.Image = null; }
        }
    }

    private void ShowPreview(string relativePath)
    {
        txtImage.Text = relativePath;
        picPreview.Image = ImageHelper.LoadImage(relativePath);
    }

    private void SaveProduct()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtBarcode.Text))
        {
            MessageBox.Show("Nombre y código son requeridos", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }
        Product.Name = txtName.Text.Trim();
        Product.Barcode = txtBarcode.Text.Trim();
        Product.Cost = nudCost.Value;
        Product.Price = nudPrice.Value;
        Product.Stock = (int)nudStock.Value;
        Product.Alerts = (int)nudAlerts.Value;
        if (cmbCategory.SelectedIndex >= 0)
            Product.CategoryId = Categories[cmbCategory.SelectedIndex].Id;
        if (_selectedImagePath != null)
        {
            var oldImage = Product.Image;
            Product.Image = ImageHelper.SaveImage(_selectedImagePath, "products");
            if (!string.IsNullOrEmpty(oldImage) && oldImage != Product.Image)
                ImageHelper.DeleteImage(oldImage);
        }
    }
}
