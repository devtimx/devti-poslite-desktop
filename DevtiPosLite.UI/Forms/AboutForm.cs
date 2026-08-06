using System.Diagnostics;
using DevtiPosLite.UI.Helpers;

namespace DevtiPosLite.UI.Forms;

public partial class AboutForm : Form
{
    private const string AppUrl = "https://devti.dev";
    private const string SupportEmail = "devti.mx@gmail.com";

    public AboutForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        UITheme.StyleForm(this, "Acerca de", new Size(600, 400));
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;

        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            Padding = new Padding(16),
            BackColor = UITheme.Background
        };
        body.RowCount = 4;
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));

        var header = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.Surface, Padding = new Padding(16) };
        var lblTitle = new Label
        {
            Text = "Devti POS Lite",
            Font = UITheme.FontTitle,
            ForeColor = UITheme.Primary,
            AutoSize = true,
            Location = new Point(16, 12)
        };
        var version = typeof(AboutForm).Assembly.GetName().Version;
        var lblVersion = new Label
        {
            Text = version != null ? $"Versión {version.Major}.{version.Minor}.{version.Build}" : "",
            Font = UITheme.FontSmall,
            ForeColor = UITheme.TextMuted,
            AutoSize = true,
            Location = new Point(17, 46)
        };
        header.Controls.Add(lblTitle);
        header.Controls.Add(lblVersion);
        body.Controls.Add(header, 0, 0);

        var license = new GroupBox { Text = "Licencia", Dock = DockStyle.Fill, BackColor = UITheme.Surface, Padding = new Padding(12) };
        UITheme.StyleGroupBox(license);
        license.Controls.Add(new Label
        {
            Text = "Este software es un punto de venta ligero desarrollado por Devti.\n"
                 + "Licencia de uso: 1 instalación por licencia adquirida.\n\n"
                 + "Queda prohibida la redistribución o reventa del software sin\n"
                 + "autorización expresa de Devti.",
            Font = UITheme.FontNormal,
            ForeColor = UITheme.Text,
            Dock = DockStyle.Top,
            Height = 110
        });
        body.Controls.Add(license, 0, 1);

        var support = new GroupBox { Text = "Soporte y Contacto", Dock = DockStyle.Fill, BackColor = UITheme.Surface, Padding = new Padding(12) };
        UITheme.StyleGroupBox(support);

        var tlp = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, Padding = new Padding(0), BackColor = UITheme.Surface };
        tlp.RowCount = 2;
        tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        var lblWeb = new Label { Text = "Sitio web:", Font = UITheme.FontBold, ForeColor = UITheme.Text, AutoSize = true, Margin = new Padding(0, 12, 0, 0) };
        var lnkWeb = new LinkLabel
        {
            Text = AppUrl,
            Font = UITheme.FontNormal,
            LinkColor = UITheme.Primary,
            ActiveLinkColor = UITheme.PrimaryDark,
            AutoSize = true,
            Margin = new Padding(0, 12, 0, 0)
        };
        lnkWeb.LinkClicked += (s, e) => OpenUrl(AppUrl);

        var lblEmail = new Label { Text = "Correo:", Font = UITheme.FontBold, ForeColor = UITheme.Text, AutoSize = true, Margin = new Padding(0, 0, 0, 0) };
        var lnkEmail = new LinkLabel
        {
            Text = SupportEmail,
            Font = UITheme.FontNormal,
            LinkColor = UITheme.Primary,
            ActiveLinkColor = UITheme.PrimaryDark,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 0)
        };
        lnkEmail.LinkClicked += (s, e) => OpenUrl($"mailto:{SupportEmail}");

        var rowWeb = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, BackColor = UITheme.Surface };
        rowWeb.Controls.Add(lblWeb);
        rowWeb.Controls.Add(lnkWeb);
        var rowEmail = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, BackColor = UITheme.Surface };
        rowEmail.Controls.Add(lblEmail);
        rowEmail.Controls.Add(lnkEmail);

        tlp.Controls.Add(rowWeb, 0, 0);
        tlp.Controls.Add(rowEmail, 0, 1);
        support.Controls.Add(tlp);
        body.Controls.Add(support, 0, 2);

        var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 12, 0, 0), BackColor = UITheme.Background };
        var btnClose = new Button { Text = "Cerrar", Width = 120, Height = 38 };
        UITheme.StyleButton(btnClose, ButtonStyle.Primary);
        btnClose.Click += (s, e) => Close();
        footer.Controls.Add(btnClose);
        body.Controls.Add(footer, 0, 3);

        Controls.Add(body);
        AcceptButton = btnClose;
    }

    private static void OpenUrl(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { }
    }
}
