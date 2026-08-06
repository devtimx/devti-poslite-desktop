using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.Core.Models;
using DevtiPosLite.UI.Helpers;

namespace DevtiPosLite.UI.Forms;

public partial class TicketPreviewForm : Form
{
    public TicketPreviewForm(IConfigService configService, Sale sale, List<SaleDetail> details, string attendedBy = "", bool autoPrint = false, string printerName = "", int copies = 1)
    {
        InitializeComponent();
        if (autoPrint)
        {
            Load += async (s, e) =>
            {
                await BuildTicketAndPrint(configService, sale, details, attendedBy, printerName, copies);
                Close();
            };
        }
        else
        {
            Load += async (s, e) => await BuildTicket(configService, sale, details, attendedBy);
        }
    }

    private async Task BuildTicketAndPrint(IConfigService configService, Sale sale, List<SaleDetail> details, string attendedBy, string printerName, int copies)
    {
        await BuildTicket(configService, sale, details, attendedBy);
        Print(printerName, copies);
    }

    private async Task BuildTicket(IConfigService configService, Sale sale, List<SaleDetail> details, string attendedBy)
    {
        var config = await configService.GetConfigAsync();
        var sb = new System.Text.StringBuilder();

        if (!string.IsNullOrWhiteSpace(config.LogoPath))
        {
            var logo = ImageHelper.ResolvePath(config.LogoPath);
            if (logo != null)
            {
                try { picLogo.Image = Image.FromFile(logo); } catch { }
            }
        }

        var storeName = !string.IsNullOrWhiteSpace(config.StoreName) ? config.StoreName : config.BusinessName;
        if (!string.IsNullOrWhiteSpace(storeName))
            sb.AppendLine(storeName.ToUpperInvariant());
        if (!string.IsNullOrWhiteSpace(config.RFC))
            sb.AppendLine($"RFC: {config.RFC}");
        if (!string.IsNullOrWhiteSpace(config.Address))
            sb.AppendLine(config.Address);
        if (!string.IsNullOrWhiteSpace(config.Phone))
            sb.AppendLine($"Tel: {config.Phone}");

        if (!string.IsNullOrWhiteSpace(config.TicketHeader))
            sb.AppendLine(config.TicketHeader);

        sb.AppendLine($"No. TICKET: VT-{sale.Id:D5}");
        sb.AppendLine($"Fecha: {sale.CreatedAt:dd/MM/yyyy}   Hora: {sale.CreatedAt:HH:mm:ss}");
        sb.AppendLine(new string('-', 36));
        sb.AppendLine("Cant  Descripción          Importe");
        sb.AppendLine(new string('-', 36));

        if (config.ShowIVABreakdown)
        {
            foreach (var d in details)
                sb.AppendLine($"{d.Quantity,-5}{d.Product?.Name ?? $"Prod#{d.ProductId}",-20}${d.Price * d.Quantity,8:N2}");
            var subtotal = sale.Total / (1 + config.IVARate);
            var iva = sale.Total - subtotal;
            sb.AppendLine(new string('-', 36));
            sb.AppendLine($"Subtotal:                  ${subtotal,8:N2}");
            sb.AppendLine($"IVA ({config.IVARate * 100:N0}%):                 ${iva,8:N2}");
            sb.AppendLine($"TOTAL:                     ${sale.Total,8:N2}");
        }
        else
        {
            foreach (var d in details)
                sb.AppendLine($"{d.Quantity,-5}{d.Product?.Name ?? $"Prod#{d.ProductId}",-20}${d.Price * d.Quantity,8:N2}");
            sb.AppendLine(new string('-', 36));
            sb.AppendLine($"TOTAL:                     ${sale.Total,8:N2}");
        }

        sb.AppendLine($"Efectivo:                  ${sale.Cash,8:N2}");
        sb.AppendLine($"Cambio:                    ${sale.Change,8:N2}");
        sb.AppendLine(new string('-', 36));

        sb.AppendLine($"Atendió: {attendedBy}");

        if (!string.IsNullOrWhiteSpace(config.TicketFooter))
            sb.AppendLine(config.TicketFooter);

        sb.AppendLine("\n¡Gracias por su compra!");
        txtTicket.Text = sb.ToString();
    }

    private void InitializeComponent()
    {
        UITheme.StyleForm(this, "Ticket de Venta", new Size(380, 650));
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimumSize = new Size(360, 420);

        var topPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 100, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(5) };
        picLogo = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, Width = 80, Height = 80, BorderStyle = BorderStyle.FixedSingle, BackColor = UITheme.Surface };
        topPanel.Controls.Add(picLogo);
        Controls.Add(topPanel);

        txtTicket = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, Font = new Font("Consolas", 10), BackColor = Color.White, ScrollBars = ScrollBars.Vertical };
        Controls.Add(txtTicket);

        var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 50, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(5) };
        var btnPrint = new Button { Text = "Imprimir", Width = 110, Height = 36 };
        UITheme.StyleButton(btnPrint, ButtonStyle.Primary);
        btnPrint.Click += (s, e) => PrintCbx();
        var btnClose = new Button { Text = "Cerrar", Width = 110, Height = 36, DialogResult = DialogResult.Cancel };
        UITheme.StyleGhostButton(btnClose);
        btnPanel.Controls.Add(btnClose);
        btnPanel.Controls.Add(btnPrint);
        Controls.Add(btnPanel);
        AcceptButton = btnClose;
    }

    private TextBox txtTicket = null!;
    private PictureBox picLogo = null!;

    private void PrintCbx()
    {
        using var cbo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
        foreach (var p in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            cbo.Items.Add(p);
        if (cbo.Items.Count > 0) cbo.SelectedIndex = 0;
        var form = new Form { Text = "Seleccionar impresora", Size = new Size(400, 150), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MinimizeBox = false, MaximizeBox = false };
        var btnOk = new Button { Text = "Imprimir", DialogResult = DialogResult.OK, Location = new Point(150, 60), Size = new Size(100, 30) };
        cbo.Location = new Point(50, 20);
        form.Controls.Add(cbo);
        form.Controls.Add(btnOk);
        form.AcceptButton = btnOk;
        if (form.ShowDialog() == DialogResult.OK && cbo.SelectedItem != null)
            Print(cbo.SelectedItem.ToString()!, 1);
    }

    private void Print(string printerName, int copies)
    {
        var pd = new System.Drawing.Printing.PrintDocument();
        pd.PrinterSettings.PrinterName = printerName;
        pd.PrinterSettings.Copies = (short)Math.Max(1, copies);
        pd.PrintPage += (s, e) =>
        {
            if (e.Graphics == null) return;
            var font = new Font("Consolas", 10);
            var y = 10f;
            foreach (var line in txtTicket.Text.Split(Environment.NewLine))
            {
                e.Graphics.DrawString(line, font, Brushes.Black, 10, y);
                y += font.GetHeight() + 2;
            }
        };
        try { pd.Print(); }
        catch (Exception ex) { MessageBox.Show($"Error al imprimir: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
