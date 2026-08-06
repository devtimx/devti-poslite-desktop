using ClosedXML.Excel;
using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.Core.Models;
using DevtiPosLite.UI.Helpers;

namespace DevtiPosLite.UI.Forms;

public partial class ConfigForm : Form
{
    private readonly ICatalogService _catalogService;
    private readonly IConfigService _configService;
    private StoreConfig _config = new();
    private string? _selectedLogoPath;

    public ConfigForm(ICatalogService catalogService, IConfigService configService)
    {
        _catalogService = catalogService;
        _configService = configService;
        InitializeComponent();
    }

    private async void InitializeComponent()
    {
        UITheme.StyleForm(this, "Configuración", new Size(640, 540));
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimumSize = new Size(640, 540);

        var tabs = new TabControl { Dock = DockStyle.Fill, Font = UITheme.FontNormal };

        tabs.Controls.Add(BuildBackupTab());
        tabs.Controls.Add(BuildImportTab());
        tabs.Controls.Add(await BuildStoreTab());
        tabs.Controls.Add(BuildTicketTab());

        Controls.Add(tabs);
    }

    private TabPage BuildBackupTab()
    {
        var tab = new TabPage("Respaldo BD");
        var lbl = new Label { Text = "Copia de seguridad de la base de datos actual.", Location = new Point(15, 20), Size = new Size(400, 25), ForeColor = UITheme.TextMuted };
        var btn = new Button { Text = "Respaldar ahora", Location = new Point(15, 55), Size = new Size(170, 40) };
        UITheme.StyleButton(btn, ButtonStyle.Primary);
        btn.Click += DoBackup;
        tab.Controls.Add(lbl); tab.Controls.Add(btn);
        return tab;
    }

    private TabPage BuildImportTab()
    {
        var tab = new TabPage("Importar Excel");
        var lbl = new Label { Text = "Importar productos desde archivo Excel (.xlsx).", Location = new Point(15, 20), Size = new Size(400, 25), ForeColor = UITheme.TextMuted };
        var fmt = new Label { Text = "Formato: Nombre | Código | Categoría | Costo | Precio | Stock | Alerta", Location = new Point(15, 45), Size = new Size(450, 25), ForeColor = UITheme.TextMuted, Font = UITheme.FontSmall };
        var btnTmpl = new Button { Text = "Descargar plantilla", Location = new Point(15, 80), Size = new Size(170, 35) };
        UITheme.StyleButton(btnTmpl, ButtonStyle.Primary);
        btnTmpl.Click += DownloadTemplate;
        var btnImp = new Button { Text = "Importar archivo...", Location = new Point(200, 80), Size = new Size(170, 35) };
        UITheme.StyleButton(btnImp, ButtonStyle.Success);
        btnImp.Click += DoImport;
        tab.Controls.Add(lbl); tab.Controls.Add(fmt); tab.Controls.Add(btnTmpl); tab.Controls.Add(btnImp);
        return tab;
    }

    private async Task<TabPage> BuildStoreTab()
    {
        _config = await _configService.GetConfigAsync();
        var tab = new TabPage("Tienda");

        var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 10, Padding = new Padding(10) };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        AddRow(tbl, "Nombre:", txtStoreName = new TextBox { Text = _config.StoreName }, 0);
        AddRow(tbl, "Razón social:", txtBusinessName = new TextBox { Text = _config.BusinessName }, 1);
        AddRow(tbl, "Teléfono:", txtPhone = new TextBox { Text = _config.Phone }, 2);
        AddRow(tbl, "RFC:", txtRFC = new TextBox { Text = _config.RFC }, 3);

        tbl.Controls.Add(new Label { Text = "Dirección:", TextAlign = ContentAlignment.TopRight, Anchor = AnchorStyles.Right, ForeColor = UITheme.TextMuted }, 0, 4);
        txtAddress = new TextBox { Text = _config.Address, Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
        tbl.Controls.Add(txtAddress, 1, 4);

        tbl.Controls.Add(new Label { Text = "IVA %:", TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Right, ForeColor = UITheme.TextMuted }, 0, 5);
        nudIVA = new NumericUpDown { DecimalPlaces = 2, Maximum = 100, Minimum = 0, Value = _config.IVARate * 100, Width = 120, Anchor = AnchorStyles.Left };
        tbl.Controls.Add(nudIVA, 1, 5);

        tbl.Controls.Add(new Label { Text = "Logo:", TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Right, ForeColor = UITheme.TextMuted }, 0, 6);
        var logoPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        txtLogo = new TextBox { ReadOnly = true, Width = 200, Text = _config.LogoPath };
        var btnLogo = new Button { Text = "Examinar", Size = new Size(90, 30), Margin = new Padding(6, 0, 0, 0) };
        UITheme.StyleGhostButton(btnLogo);
        btnLogo.Click += (s, e) =>
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _selectedLogoPath = ofd.FileName;
                txtLogo.Text = ofd.FileName;
                SetLogoImage(ofd.FileName);
            }
        };
        logoPanel.Controls.Add(txtLogo); logoPanel.Controls.Add(btnLogo);
        tbl.Controls.Add(logoPanel, 1, 6);

        picLogo = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle, Anchor = AnchorStyles.None, BackColor = UITheme.Surface, Visible = false };
        tbl.Controls.Add(picLogo, 1, 7);
        UpdateLogoPreview();

        var btnSave = new Button { Text = "Guardar configuración", Size = new Size(240, 40), Margin = new Padding(0, 4, 0, 0) };
        UITheme.StyleButton(btnSave, ButtonStyle.Primary);
        btnSave.Click += async (s, e) => await SaveStoreConfig();
        tbl.SetColumnSpan(btnSave, 2);
        tbl.Controls.Add(btnSave, 0, 9);

        tab.Controls.Add(tbl);
        return tab;
    }

    private TabPage BuildTicketTab()
    {
        var tab = new TabPage("Ticket");
        var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 10, Padding = new Padding(10) };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        tbl.Controls.Add(new Label { Text = "Encabezado:", TextAlign = ContentAlignment.TopRight, Anchor = AnchorStyles.Right, ForeColor = UITheme.TextMuted }, 0, 0);
        txtTicketHeader = new TextBox { Text = _config.TicketHeader, Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
        tbl.Controls.Add(txtTicketHeader, 1, 0);

        tbl.Controls.Add(new Label { Text = "Pie de página:", TextAlign = ContentAlignment.TopRight, Anchor = AnchorStyles.Right, ForeColor = UITheme.TextMuted }, 0, 1);
        txtTicketFooter = new TextBox { Text = _config.TicketFooter, Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
        tbl.Controls.Add(txtTicketFooter, 1, 1);

        chkPrintTicket = new CheckBox { Text = "Generar ticket al completar venta/devolución", Checked = _config.PrintTicket, Anchor = AnchorStyles.Left };
        tbl.Controls.Add(chkPrintTicket, 1, 2);

        chkIVABreakdown = new CheckBox { Text = "Desglosar IVA en el ticket", Checked = _config.ShowIVABreakdown, Anchor = AnchorStyles.Left };
        tbl.Controls.Add(chkIVABreakdown, 1, 3);

        tbl.Controls.Add(new Label { Text = "Impresora:", TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Right, ForeColor = UITheme.TextMuted }, 0, 4);
        var printerPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        cboPrinters = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 250 };
        foreach (var printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            cboPrinters.Items.Add(printer);
        if (!string.IsNullOrEmpty(_config.DefaultPrinter) && cboPrinters.Items.Contains(_config.DefaultPrinter))
            cboPrinters.SelectedItem = _config.DefaultPrinter;
        else if (cboPrinters.Items.Count > 0)
            cboPrinters.SelectedIndex = 0;
        var btnRefreshPrinters = new Button { Text = "Refrescar", Size = new Size(90, 30), Margin = new Padding(6, 0, 0, 0) };
        UITheme.StyleGhostButton(btnRefreshPrinters);
        btnRefreshPrinters.Click += (s, e) =>
        {
            cboPrinters.Items.Clear();
            foreach (var p in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                cboPrinters.Items.Add(p);
            if (cboPrinters.Items.Count > 0) cboPrinters.SelectedIndex = 0;
        };
        printerPanel.Controls.Add(cboPrinters);
        printerPanel.Controls.Add(btnRefreshPrinters);
        tbl.Controls.Add(printerPanel, 1, 4);

        chkAutoPrint = new CheckBox { Text = "Impresión automática (sin mostrar vista previa)", Checked = _config.AutoPrint, Anchor = AnchorStyles.Left };
        tbl.Controls.Add(chkAutoPrint, 1, 5);

        tbl.Controls.Add(new Label { Text = "Copias:", TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Right, ForeColor = UITheme.TextMuted }, 0, 6);
        var copiesPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        nudCopies = new NumericUpDown { Minimum = 1, Maximum = 10, Value = _config.PrintCopies, Width = 60 };
        copiesPanel.Controls.Add(nudCopies);
        copiesPanel.Controls.Add(new Label { Text = " (1 = ticket simple, 2 = original + copia)", ForeColor = UITheme.TextMuted });
        tbl.Controls.Add(copiesPanel, 1, 6);

        var lblPreview = new Label { Text = "Vista previa:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Anchor = AnchorStyles.Left };
        tbl.Controls.Add(lblPreview, 1, 7);

        txtPreview = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, Font = new Font("Consolas", 9), BackColor = Color.White };
        tbl.Controls.Add(txtPreview, 1, 8);
        UpdateTicketPreview();

        chkIVABreakdown.CheckedChanged += (s, e) => UpdateTicketPreview();
        chkPrintTicket.CheckedChanged += (s, e) => UpdateTicketPreview();

        var btnSaveTicket = new Button { Text = "Guardar configuración de ticket", Size = new Size(280, 40), Margin = new Padding(0, 4, 0, 0) };
        UITheme.StyleButton(btnSaveTicket, ButtonStyle.Primary);
        btnSaveTicket.Click += async (s, e) => await SaveTicketConfig();
        tbl.SetColumnSpan(btnSaveTicket, 2);
        tbl.Controls.Add(btnSaveTicket, 0, 9);

        tab.Controls.Add(tbl);
        return tab;
    }

    private TextBox txtStoreName = null!, txtBusinessName = null!, txtPhone = null!, txtRFC = null!, txtAddress = null!, txtLogo = null!, txtTicketHeader = null!, txtTicketFooter = null!, txtPreview = null!;
    private NumericUpDown nudIVA = null!, nudCopies = null!;
    private PictureBox picLogo = null!;
    private CheckBox chkPrintTicket = null!, chkIVABreakdown = null!, chkAutoPrint = null!;
    private ComboBox cboPrinters = null!;

    private void AddRow(TableLayoutPanel tbl, string label, Control ctrl, int row)
    {
        tbl.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Right, ForeColor = UITheme.TextMuted }, 0, row);
        ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        tbl.Controls.Add(ctrl, 1, row);
    }

    private void SetLogoImage(string path)
    {
        try
        {
            var img = Image.FromFile(path);
            if (picLogo.Image != null) picLogo.Image.Dispose();
            picLogo.Image = img;
            picLogo.Size = ImageHelper.FitSize(img.Width, img.Height, 400, 400);
            picLogo.Visible = true;
        }
        catch { picLogo.Visible = false; }
    }

    private void UpdateLogoPreview()
    {
        var savedLogo = ImageHelper.ResolveLogoPath(_config.LogoPath);
        if (savedLogo != null)
            SetLogoImage(savedLogo);
    }

    private void UpdateTicketPreview()
    {
        var ivaRate = _config.IVARate;
        var showIVA = chkIVABreakdown.Checked;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== " + (!string.IsNullOrWhiteSpace(txtTicketHeader.Text) ? txtTicketHeader.Text : "MI TIENDA") + " ===");
        sb.AppendLine("------------------------------");
        sb.AppendLine("Prod        Cant   Precio");
        sb.AppendLine("------------------------------");
        sb.AppendLine("Producto A   2      $100.00");
        sb.AppendLine("Producto B   1       $50.00");
        sb.AppendLine("------------------------------");
        if (showIVA)
        {
            var subtotal = 250m / (1 + ivaRate);
            var iva = 250m - subtotal;
            sb.AppendLine($"Subtotal:          ${subtotal:N2}");
            sb.AppendLine($"IVA ({ivaRate * 100:N0}%):         ${iva:N2}");
        }
        sb.AppendLine($"Total:             $250.00");
        sb.AppendLine("------------------------------");
        if (!string.IsNullOrWhiteSpace(txtTicketFooter.Text))
            sb.AppendLine(txtTicketFooter.Text);
        sb.AppendLine(chkPrintTicket.Checked ? "[IMPRIMIR]" : "[NO IMPRIMIR]");
        txtPreview.Text = sb.ToString();
    }

    private async Task SaveStoreConfig()
    {
        if (_selectedLogoPath != null)
        {
            var oldLogo = _config.LogoPath;
            _config.LogoPath = ImageHelper.SaveImage(_selectedLogoPath, "logos");
            if (!string.IsNullOrEmpty(oldLogo) && oldLogo != _config.LogoPath)
                ImageHelper.DeleteImage(oldLogo);
        }
        _config.StoreName = txtStoreName.Text.Trim();
        _config.BusinessName = txtBusinessName.Text.Trim();
        _config.Phone = txtPhone.Text.Trim();
        _config.RFC = txtRFC.Text.Trim();
        _config.Address = txtAddress.Text.Trim();
        _config.IVARate = nudIVA.Value / 100m;
        await _configService.SaveConfigAsync(_config);
        MessageBox.Show("Configuración guardada.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task SaveTicketConfig()
    {
        _config.TicketHeader = txtTicketHeader.Text;
        _config.TicketFooter = txtTicketFooter.Text;
        _config.PrintTicket = chkPrintTicket.Checked;
        _config.ShowIVABreakdown = chkIVABreakdown.Checked;
        _config.DefaultPrinter = cboPrinters.SelectedItem?.ToString() ?? "";
        _config.AutoPrint = chkAutoPrint.Checked;
        _config.PrintCopies = (int)nudCopies.Value;
        await _configService.SaveConfigAsync(_config);
        MessageBox.Show("Configuración de ticket guardada.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void DownloadTemplate(object? sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog();
        sfd.Filter = "Excel|*.xlsx";
        sfd.FileName = "plantilla_productos.xlsx";
        if (sfd.ShowDialog() != DialogResult.OK) return;
        try
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Productos");
            ws.Cell(1, 1).Value = "Nombre";
            ws.Cell(1, 2).Value = "Código";
            ws.Cell(1, 3).Value = "Categoría";
            ws.Cell(1, 4).Value = "Costo";
            ws.Cell(1, 5).Value = "Precio";
            ws.Cell(1, 6).Value = "Stock";
            ws.Cell(1, 7).Value = "Alerta";
            var hr = ws.Range(1, 1, 1, 7);
            hr.Style.Font.Bold = true;
            hr.Style.Fill.BackgroundColor = XLColor.LightGray;
            hr.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            hr.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Cell(2, 1).Value = "Ejemplo";
            ws.Cell(2, 2).Value = "ABC123";
            ws.Cell(2, 3).Value = "General";
            ws.Cell(2, 4).Value = 50;
            ws.Cell(2, 5).Value = 100;
            ws.Cell(2, 6).Value = 10;
            ws.Cell(2, 7).Value = 5;
            ws.Columns().AdjustToContents();
            workbook.SaveAs(sfd.FileName);
            MessageBox.Show("Plantilla guardada.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void DoBackup(object? sender, EventArgs e)
    {
        try
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var dbPath = Path.Combine(basePath, "poslite.db");
            if (!File.Exists(dbPath)) { MessageBox.Show("No se encontró la BD", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            var backupDir = Path.Combine(basePath, "backups");
            Directory.CreateDirectory(backupDir);
            var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var dest = Path.Combine(backupDir, $"poslite_{ts}.db");
            File.Copy(dbPath, dest, true);
            MessageBox.Show($"Respaldo: {dest}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async void DoImport(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog();
        ofd.Filter = "Excel|*.xlsx";
        if (ofd.ShowDialog() != DialogResult.OK) return;
        try
        {
            var categories = (await _catalogService.GetCategoriesAsync()).ToList();
            int imported = 0, updated = 0;
            using var workbook = new XLWorkbook(ofd.FileName);
            var ws = workbook.Worksheet(1);
            var range = ws.RangeUsed();
            if (range == null) { MessageBox.Show("Excel vacío", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            foreach (var row in range.RowsUsed().Skip(1))
            {
                var name = row.Cell(1).GetString().Trim();
                var barcode = row.Cell(2).GetString().Trim();
                var catName = row.Cell(3).GetString().Trim();
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(barcode)) continue;
                var category = categories.FirstOrDefault(c => c.Name.Equals(catName, StringComparison.OrdinalIgnoreCase));
                if (category == null)
                {
                    category = await _catalogService.CreateCategoryAsync(new Category { Name = catName });
                    categories.Add(category);
                }
                var allProds = await _catalogService.GetProductsAsync();
                var existing = allProds.FirstOrDefault(p => p.Barcode == barcode);
                if (existing != null)
                {
                    existing.Name = name; existing.CategoryId = category.Id;
                    existing.Cost = (decimal)row.Cell(4).GetDouble();
                    existing.Price = (decimal)row.Cell(5).GetDouble();
                    existing.Stock = (int)row.Cell(6).GetValue<double>();
                    existing.Alerts = (int)row.Cell(7).GetValue<double>();
                    await _catalogService.UpdateProductAsync(existing);
                    updated++;
                }
                else
                {
                    await _catalogService.CreateProductAsync(new Product
                    {
                        Name = name, Barcode = barcode, CategoryId = category.Id,
                        Cost = (decimal)row.Cell(4).GetDouble(), Price = (decimal)row.Cell(5).GetDouble(),
                        Stock = (int)row.Cell(6).GetValue<double>(), Alerts = (int)row.Cell(7).GetValue<double>()
                    });
                    imported++;
                }
            }
            MessageBox.Show($"Importación: {imported} nuevos, {updated} actualizados.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
