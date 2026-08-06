using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.Core.Models;
using DevtiPosLite.UI.Helpers;
using DevtiPosLite.UI.State;

namespace DevtiPosLite.UI.Forms;

public partial class PosForm : Form
{
    private readonly ICatalogService _catalogService;
    private readonly ISalesService _salesService;
    private readonly ICashierService _cashierService;
    private readonly IConfigService _configService;
    private readonly AuthStore _authStore;

    private readonly BindingSource _cartBinding = new();
    private List<CartItem> _cartItems = new();
    private List<Product> _products = new();
    private List<Denomination> _denominations = new();
    private CashOpening? _currentCashOpening;

    public PosForm(
        ICatalogService catalogService,
        ISalesService salesService,
        ICashierService cashierService,
        IConfigService configService,
        AuthStore authStore)
    {
        InitializeComponent();
        _catalogService = catalogService;
        _salesService = salesService;
        _cashierService = cashierService;
        _configService = configService;
        _authStore = authStore;
    }

    private class CartItem
    {
        public uint ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => Price * Quantity;
    }

    private async void InitializeComponent()
    {
        UITheme.StyleForm(this, "Punto de Venta", new Size(1200, 700));

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterWidth = 6,
            SplitterDistance = 660
        };

        var leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        txtSearch = new TextBox
        {
            Dock = DockStyle.Top,
            Height = 36,
            PlaceholderText = "Buscar producto por nombre o código...",
            Font = UITheme.FontNormal
        };
        UITheme.StyleTextBox(txtSearch);
        txtSearch.TextChanged += async (s, e) => await LoadProductsAsync(txtSearch.Text);

        flowProducts = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };

        leftPanel.Controls.Add(flowProducts);
        leftPanel.Controls.Add(txtSearch);
        split.Panel1.Controls.Add(leftPanel);

        var rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 168));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

        var lblCart = UITheme.CreateTitle("Carrito");
        lblCart.Dock = DockStyle.Fill;
        lblCart.Font = new Font("Segoe UI", 13, FontStyle.Bold);
        layout.Controls.Add(lblCart, 0, 0);

        dgvCart = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        UITheme.StyleDataGrid(dgvCart);
        dgvCart.Columns.Add("ProductName", "Producto");
        dgvCart.Columns.Add("Quantity", "Cant");
        dgvCart.Columns.Add("Price", "Precio");
        dgvCart.Columns.Add("Subtotal", "Subtotal");
        dgvCart.Columns["Subtotal"]!.DefaultCellStyle.Format = "N2";
        dgvCart.Columns["Price"]!.DefaultCellStyle.Format = "N2";
        layout.Controls.Add(dgvCart, 0, 1);

        var btnRemove = new Button { Text = "Quitar seleccionado", Size = new Size(170, 34), Anchor = AnchorStyles.Left };
        UITheme.StyleGhostButton(btnRemove);
        btnRemove.Click += (s, e) => RemoveFromCart();
        var removeRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        removeRow.Controls.Add(btnRemove);
        layout.Controls.Add(removeRow, 0, 2);

        var lblTotal = new Label { Text = "Total:", Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true };
        lblTotalValue = new Label { Text = "$0.00", Font = new Font("Segoe UI", 15, FontStyle.Bold), AutoSize = true, ForeColor = UITheme.SuccessDark };
        var totalRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        totalRow.Controls.Add(lblTotal);
        totalRow.Controls.Add(lblTotalValue);
        layout.Controls.Add(totalRow, 0, 3);

        var denomGroup = new GroupBox { Text = "Efectivo recibido", Dock = DockStyle.Fill };
        UITheme.StyleGroupBox(denomGroup);
        flowDenominations = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 62, AutoScroll = true };
        txtCash = new TextBox { Width = 130, Text = "0", TextAlign = HorizontalAlignment.Right, Font = UITheme.FontBold };
        UITheme.StyleTextBox(txtCash);
        var btnExact = new Button { Text = "Exacto", Width = 80, Height = 30 };
        UITheme.StyleGhostButton(btnExact);
        btnExact.Click += (s, e) => txtCash.Text = _cartItems.Sum(i => i.Subtotal).ToString("N2");
        var lblChange = new Label { Text = "Cambio:", ForeColor = UITheme.TextMuted, AutoSize = true };
        lblChangeValue = new Label { Text = "$0.00", ForeColor = UITheme.Danger, Font = UITheme.FontBold, AutoSize = true };
        txtCash.TextChanged += (s, e) => UpdateChange();
        var cashRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        cashRow.Controls.Add(txtCash);
        cashRow.Controls.Add(btnExact);
        cashRow.Controls.Add(lblChange);
        cashRow.Controls.Add(lblChangeValue);
        denomGroup.Controls.Add(cashRow);
        denomGroup.Controls.Add(flowDenominations);
        layout.Controls.Add(denomGroup, 0, 4);

        var btnPay = new Button { Dock = DockStyle.Fill };
        UITheme.StyleButton(btnPay, ButtonStyle.Success);
        btnPay.Text = "COBRAR";
        btnPay.Font = new Font("Segoe UI", 13, FontStyle.Bold);
        btnPay.Click += async (s, e) => await PayAsync();
        layout.Controls.Add(btnPay, 0, 5);

        var btnReturn = new Button { Text = "Devolución (F8)", Size = new Size(150, 34), Anchor = AnchorStyles.Left };
        UITheme.StyleGhostButton(btnReturn);
        btnReturn.Click += async (s, e) => await OpenReturnDialog();
        var btnReprint = new Button { Text = "Reimprimir ticket", Size = new Size(150, 34), Anchor = AnchorStyles.Left };
        UITheme.StyleGhostButton(btnReprint);
        btnReprint.Click += async (s, e) => await ReprintTicket();
        var payRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        payRow.Controls.Add(btnReturn);
        payRow.Controls.Add(btnReprint);
        layout.Controls.Add(payRow, 0, 6);

        rightPanel.Controls.Add(layout);
        split.Panel2.Controls.Add(rightPanel);

        Controls.Add(split);

        split.Resize += (s, e) => UITheme.KeepSplitRatio(split, 0.55);

        Load += async (s, e) => await LoadDataAsync();
    }

    private TextBox txtSearch = null!;
    private FlowLayoutPanel flowProducts = null!;
    private DataGridView dgvCart = null!;
    private Label lblTotalValue = null!;
    private FlowLayoutPanel flowDenominations = null!;
    private TextBox txtCash = null!;
    private Label lblChangeValue = null!;

    private async Task LoadDataAsync()
    {
        _products = (await _catalogService.GetProductsAsync()).ToList();
        _denominations = (await _catalogService.GetDenominationsAsync()).ToList();
        PopulateProductGrid();
        PopulateDenominationButtons();

        if (_authStore.UserId.HasValue)
        {
            _currentCashOpening = await _cashierService.GetCurrentCashOpeningAsync(_authStore.UserId.Value);
            if (_currentCashOpening == null)
            {
                var result = MessageBox.Show("Debe abrir la caja antes de vender. ¿Abrir caja ahora?",
                    "Caja Cerrada", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                    await OpenCashDialogAsync();
            }
        }
    }

    private async Task LoadProductsAsync(string? search)
    {
        _products = (await _catalogService.GetProductsAsync(search)).ToList();
        PopulateProductGrid();
    }

    private void PopulateProductGrid()
    {
        flowProducts.Controls.Clear();
        foreach (var p in _products)
        {
            var lowStock = p.Stock <= p.Alerts;
            var btn = new Button
            {
                Text = $"{p.Name}\n${p.Price:N2}\nStock: {p.Stock}",
                Size = new Size(150, 86),
                Margin = new Padding(5),
                BackColor = lowStock ? Color.FromArgb(254, 226, 226) : Color.White,
                ForeColor = lowStock ? Color.FromArgb(153, 27, 27) : UITheme.Text,
                Tag = p,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                TextAlign = ContentAlignment.BottomCenter,
                TextImageRelation = TextImageRelation.ImageAboveText,
                ImageAlign = ContentAlignment.TopCenter,
                Padding = new Padding(3),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = lowStock ? Color.FromArgb(254, 202, 202) : UITheme.Border;
            btn.FlatAppearance.BorderSize = 1;
            var img = ImageHelper.LoadImage(p.Image, 145, 55);
            if (img != null) btn.Image = img;
            btn.Click += (s, e) => AddToCart(p);
            flowProducts.Controls.Add(btn);
        }
    }

    private void PopulateDenominationButtons()
    {
        flowDenominations.Controls.Clear();
        foreach (var d in _denominations)
        {
            var isBill = d.Type == "BILLETE";
            var btn = new Button
            {
                Text = $"${d.Value:N2}",
                Size = new Size(78, 50),
                Margin = new Padding(3),
                Tag = d.Value,
                BackColor = isBill ? Color.FromArgb(219, 234, 254) : Color.FromArgb(254, 249, 195),
                ForeColor = UITheme.Text,
                TextImageRelation = TextImageRelation.ImageAboveText,
                ImageAlign = ContentAlignment.TopCenter,
                TextAlign = ContentAlignment.BottomCenter,
                Padding = new Padding(1),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = isBill ? Color.FromArgb(191, 219, 254) : Color.FromArgb(254, 240, 138);
            btn.FlatAppearance.BorderSize = 1;
            var img = ImageHelper.LoadImage(d.Image, 66, 24);
            if (img != null) btn.Image = img;
            btn.Click += (s, e) =>
            {
                decimal val = (decimal)((Button)s!).Tag!;
                txtCash.Text = (decimal.Parse(txtCash.Text.Replace("$", "").Replace(",", "")) + val).ToString("N2");
            };
            flowDenominations.Controls.Add(btn);
        }
    }

    private void AddToCart(Product product)
    {
        var existing = _cartItems.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing != null)
        {
            if (existing.Quantity >= product.Stock)
            {
                MessageBox.Show("Stock insuficiente", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            existing.Quantity++;
        }
        else
        {
            if (product.Stock <= 0)
            {
                MessageBox.Show("Producto sin stock", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _cartItems.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = 1
            });
        }
        RefreshCart();
    }

    private void RemoveFromCart()
    {
        if (dgvCart.CurrentRow != null && dgvCart.CurrentRow.Index < _cartItems.Count)
        {
            _cartItems.RemoveAt(dgvCart.CurrentRow.Index);
            RefreshCart();
        }
    }

    private void RefreshCart()
    {
        dgvCart.Rows.Clear();
        foreach (var item in _cartItems)
            dgvCart.Rows.Add(item.ProductName, item.Quantity, item.Price, item.Subtotal);
        lblTotalValue.Text = $"${_cartItems.Sum(i => i.Subtotal):N2}";
        UpdateChange();
    }

    private void UpdateChange()
    {
        if (decimal.TryParse(txtCash.Text.Replace("$", "").Replace(",", ""), out decimal cash))
        {
            var total = _cartItems.Sum(i => i.Subtotal);
            var change = cash - total;
            lblChangeValue.Text = change > 0 ? $"${change:N2}" : "$0.00";
            lblChangeValue.ForeColor = change >= 0 ? Color.Red : Color.DarkRed;
        }
    }

    private async Task PayAsync()
    {
        if (_cartItems.Count == 0)
        {
            MessageBox.Show("Carrito vacío", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!decimal.TryParse(txtCash.Text.Replace("$", "").Replace(",", ""), out decimal cash) || cash <= 0)
        {
            MessageBox.Show("Monto de efectivo inválido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var request = new Core.DTOs.SaleRequest
            {
                Cash = cash,
                Items = _cartItems.Select(i => new Core.DTOs.SaleItemRequest
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };

            var sale = await _salesService.CreateSaleAsync(request, _authStore.UserId!.Value);

            var config = await _configService.GetConfigAsync();
            if (config.PrintTicket)
            {
                var fullSale = await _salesService.GetSaleWithDetailsAsync(sale.Id);
                if (fullSale != null)
                {
                    var userName = _authStore.CurrentUser?.Name ?? "";
                    if (config.AutoPrint)
                    {
                        using var ticket = new TicketPreviewForm(_configService, fullSale, fullSale.Details.ToList(), userName,
                            autoPrint: true, printerName: config.DefaultPrinter, copies: config.PrintCopies);
                        ticket.ShowDialog();
                    }
                    else
                    {
                        new TicketPreviewForm(_configService, fullSale, fullSale.Details.ToList(), userName).ShowDialog();
                    }
                }
            }
            else
            {
                MessageBox.Show("Venta realizada con éxito", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            _cartItems.Clear();
            RefreshCart();
            txtCash.Text = "0";
            await LoadProductsAsync(txtSearch.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task OpenCashDialogAsync()
    {
        var dialog = new Dialogs.CashOpeningDialog();
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            await _cashierService.OpenCashAsync(new Core.DTOs.CashOpenRequest
            {
                OpeningAmount = dialog.OpeningAmount
            }, _authStore.UserId!.Value);
            _currentCashOpening = await _cashierService.GetCurrentCashOpeningAsync(_authStore.UserId.Value);
        }
    }

    private async Task OpenReturnDialog()
    {
        var dialog = new Dialogs.ReturnDialog(_salesService);
        if (dialog.ShowDialog() == DialogResult.OK && dialog.ReturnSaleId.HasValue && dialog.ReturnProductId.HasValue)
        {
            try
            {
                await _salesService.CreateReturnAsync(new Core.DTOs.ReturnRequest
                {
                    SaleId = dialog.ReturnSaleId.Value,
                    ProductId = dialog.ReturnProductId.Value,
                    Quantity = dialog.ReturnQuantity
                }, _authStore.UserId!.Value);
                MessageBox.Show("Devolución registrada", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadProductsAsync(txtSearch.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async Task ReprintTicket()
    {
        var folioStr = Microsoft.VisualBasic.Interaction.InputBox("Ingrese el folio del ticket a reimprimir:", "Reimprimir Ticket", "");
        if (string.IsNullOrWhiteSpace(folioStr)) return;

        if (!uint.TryParse(folioStr.Trim().Replace("VT-", "").Replace("vt-", ""), out uint saleId))
        {
            MessageBox.Show("Folio inválido. Ingrese el número de ticket.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var sale = await _salesService.GetSaleWithDetailsAsync(saleId);
        if (sale == null)
        {
            MessageBox.Show("Ticket no encontrado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var config = await _configService.GetConfigAsync();
        if (config.AutoPrint)
        {
            using var ticket = new TicketPreviewForm(_configService, sale, sale.Details.ToList(), sale.User?.Name ?? "",
                autoPrint: true, printerName: config.DefaultPrinter, copies: 1);
            ticket.ShowDialog();
        }
        else
        {
            new TicketPreviewForm(_configService, sale, sale.Details.ToList(), sale.User?.Name ?? "").ShowDialog();
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F8)
        {
            _ = OpenReturnDialog();
            return true;
        }
        if (keyData == Keys.Escape)
        {
            Close();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
