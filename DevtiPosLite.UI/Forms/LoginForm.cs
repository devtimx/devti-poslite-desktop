using DevtiPosLite.Core.DTOs;
using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.UI.Helpers;
using DevtiPosLite.UI.State;

namespace DevtiPosLite.UI.Forms;

public partial class LoginForm : Form
{
    private readonly IAuthService _authService;
    private readonly IConfigService _configService;
    private readonly AuthStore _authStore;

    public LoginForm(IAuthService authService, IConfigService configService, AuthStore authStore)
    {
        _authService = authService;
        _configService = configService;
        _authStore = authStore;
        InitializeComponent();
        Load += async (s, e) => await LoadStoreInfo();
    }

    private void InitializeComponent()
    {
        UITheme.StyleForm(this, "Devti POS Lite - Iniciar Sesión", new Size(400, 800));
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimumSize = new Size(400, 800);

        topPanel = new Panel { Dock = DockStyle.Top, Height = 400, BackColor = UITheme.Surface };
        picLogo = new PictureBox
        {
            SizeMode = PictureBoxSizeMode.Zoom,
            Anchor = AnchorStyles.None,
            Visible = false
        };
        topPanel.Controls.Add(picLogo);
        topPanel.Resize += (s, e) => CenterLogo();

        var bottomPanel = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.Background };
        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            Padding = new Padding(40, 20, 40, 12)
        };
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));

        lblStore = new Label
        {
            Text = "Devti POS Lite",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = UITheme.Primary,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };
        tbl.Controls.Add(lblStore, 0, 0);

        tbl.Controls.Add(new Label { Text = "Usuario", Font = UITheme.FontSmallBold, ForeColor = UITheme.TextMuted, Dock = DockStyle.Fill }, 0, 1);
        txtUsername = new TextBox { Font = UITheme.FontNormal, Dock = DockStyle.Fill };
        UITheme.StyleTextBox(txtUsername);
        tbl.Controls.Add(txtUsername, 0, 2);

        tbl.Controls.Add(new Label { Text = "Contraseña", Font = UITheme.FontSmallBold, ForeColor = UITheme.TextMuted, Dock = DockStyle.Fill }, 0, 3);
        txtPassword = new TextBox { UseSystemPasswordChar = true, Font = UITheme.FontNormal, Dock = DockStyle.Fill };
        UITheme.StyleTextBox(txtPassword);
        tbl.Controls.Add(txtPassword, 0, 4);

        var btnLogin = new Button
        {
            Text = "Ingresar",
            Dock = DockStyle.Fill
        };
        UITheme.StyleButton(btnLogin, ButtonStyle.Primary);
        btnLogin.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        btnLogin.Click += BtnLogin_Click;
        tbl.Controls.Add(btnLogin, 0, 5);

        lblError = new Label
        {
            Text = "",
            ForeColor = UITheme.Danger,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            Font = UITheme.FontNormal
        };
        tbl.Controls.Add(lblError, 0, 7);

        bottomPanel.Controls.Add(tbl);

        Controls.Add(bottomPanel);
        Controls.Add(topPanel);

        AcceptButton = btnLogin;
        txtPassword.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) BtnLogin_Click(s, e); };
    }

    private Panel topPanel = null!;
    private Label lblStore = null!;
    private PictureBox picLogo = null!;

    private void CenterLogo()
    {
        if (!picLogo.Visible) return;
        picLogo.Location = new Point(
            Math.Max(0, (topPanel.ClientSize.Width - picLogo.Width) / 2),
            Math.Max(0, (topPanel.ClientSize.Height - picLogo.Height) / 2));
    }

    private async Task LoadStoreInfo()
    {
        try
        {
            var config = await _configService.GetConfigAsync();
            var hasName = !string.IsNullOrWhiteSpace(config.StoreName);
            var hasLogo = !string.IsNullOrWhiteSpace(config.LogoPath);
            var storeName = hasName ? config.StoreName : config.BusinessName;

            if (hasName || !string.IsNullOrWhiteSpace(config.BusinessName))
                lblStore.Text = storeName!;

            var logoPath = ImageHelper.ResolvePath(config.LogoPath);
            if (logoPath != null)
            {
                try
                {
                    var img = Image.FromFile(logoPath);
                    if (picLogo.Image != null) picLogo.Image.Dispose();
                    picLogo.Image = img;
                    picLogo.Size = ImageHelper.FitSize(img.Width, img.Height, 400, 400);
                    picLogo.Visible = true;
                    CenterLogo();
                }
                catch { }
            }
        }
        catch { }
    }

    private TextBox txtUsername = null!;
    private TextBox txtPassword = null!;
    private Label lblError = null!;

    private async void BtnLogin_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            lblError.Text = "Ingrese usuario y contraseña";
            return;
        }

        try
        {
            var response = await _authService.LoginAsync(new LoginRequest
            {
                UserName = txtUsername.Text.Trim(),
                Password = txtPassword.Text
            });

            _authStore.SetSession(response.User, response.Token);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (UnauthorizedAccessException ex)
        {
            lblError.Text = ex.Message;
        }
        catch (Exception ex)
        {
            lblError.Text = $"Error: {ex.Message}";
        }
    }
}
