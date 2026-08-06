using DevtiPosLite.UI.Helpers;
using DevtiPosLite.UI.State;

namespace DevtiPosLite.UI.Forms;

public partial class MainForm : Form
{
    private readonly AuthStore _authStore;

    public MainForm(AuthStore authStore)
    {
        _authStore = authStore;
        InitializeComponent();
        _authStore.OnChange += OnAuthChanged;
        UpdateUIForUser();
    }

    private void InitializeComponent()
    {
        Text = "Devti POS Lite";
        WindowState = FormWindowState.Maximized;
        StartPosition = FormStartPosition.CenterScreen;
        IsMdiContainer = true;
        BackColor = UITheme.Background;

        var menu = new MenuStrip();

        menu.Items.Add(new ToolStripMenuItem("Punto de Venta (F2)", null, (s, e) => OpenForm<PosForm>()) { ShortcutKeys = Keys.F2, ShowShortcutKeys = false });

        var catalogo = new ToolStripMenuItem("Catálogos");
        catalogo.DropDownItems.Add(new ToolStripMenuItem("Productos (F3)", null, (s, e) => OpenForm<ProductsForm>()) { ShortcutKeys = Keys.F3, ShowShortcutKeys = false });
        catalogo.DropDownItems.Add(new ToolStripMenuItem("Categorías (F4)", null, (s, e) => OpenForm<CategoriesForm>()) { ShortcutKeys = Keys.F4, ShowShortcutKeys = false });
        catalogo.DropDownItems.Add(new ToolStripMenuItem("Denominaciones (F5)", null, (s, e) => OpenForm<DenominationsForm>()) { ShortcutKeys = Keys.F5, ShowShortcutKeys = false });
        menu.Items.Add(catalogo);

        var ventas = new ToolStripMenuItem("Ventas");
        ventas.DropDownItems.Add(new ToolStripMenuItem("Reportes (F6)", null, (s, e) => OpenForm<ReportsForm>()) { ShortcutKeys = Keys.F6, ShowShortcutKeys = false });
        ventas.DropDownItems.Add(new ToolStripMenuItem("Cierre de Caja (F7)", null, (s, e) => OpenForm<CashoutForm>()) { ShortcutKeys = Keys.F7, ShowShortcutKeys = false });
        ventas.DropDownItems.Add(new ToolStripMenuItem("Historial Caja", null, (s, e) => OpenForm<CashHistoryForm>()));
        menu.Items.Add(ventas);

        var admin = new ToolStripMenuItem("Administración");
        var mnuUsers = new ToolStripMenuItem("Usuarios");
        mnuUsers.Enabled = _authStore.HasPermission("Users_index");
        mnuUsers.Click += (s, e) => OpenForm<UsersForm>();
        admin.DropDownItems.Add(mnuUsers);

        var mnuRoles = new ToolStripMenuItem("Roles");
        mnuRoles.Enabled = _authStore.HasPermission("Roles_index");
        mnuRoles.Click += (s, e) => OpenForm<RolesForm>();
        admin.DropDownItems.Add(mnuRoles);

        var mnuPerms = new ToolStripMenuItem("Permisos");
        mnuPerms.Enabled = _authStore.HasPermission("Permission_index");
        mnuPerms.Click += (s, e) => OpenForm<PermissionsForm>();
        admin.DropDownItems.Add(mnuPerms);

        admin.DropDownItems.Add(new ToolStripSeparator());

        var mnuConfig = new ToolStripMenuItem("Configuración (F8)");
        mnuConfig.ShortcutKeys = Keys.F8;
        mnuConfig.ShowShortcutKeys = false;
        mnuConfig.Enabled = _authStore.HasPermission("Config_index");
        mnuConfig.Click += (s, e) => OpenForm<ConfigForm>();
        admin.DropDownItems.Add(mnuConfig);
        menu.Items.Add(admin);

        var lblUser = new ToolStripLabel($"   Usuario: {_authStore.CurrentUser?.Name ?? ""}   ");
        lblUser.Font = UITheme.FontBold;
        menu.Items.Add(lblUser);

        var btnLogout = new ToolStripMenuItem("Cerrar Sesión");
        btnLogout.Click += (s, e) => Logout();
        menu.Items.Add(btnLogout);

        UITheme.StyleMenu(menu);
        MainMenuStrip = menu;
        Controls.Add(menu);

        lblStatus = new ToolStripLabel("Listo");
        var statusStrip = new StatusStrip();
        statusStrip.BackColor = UITheme.Surface;
        statusStrip.Items.Add(lblStatus);
        Controls.Add(statusStrip);
    }

    private ToolStripLabel lblStatus = null!;

    private void OpenForm<T>() where T : Form
    {
        foreach (Form f in MdiChildren)
            if (f is T) { f.Activate(); return; }

        var form = Program.ServiceProvider!.GetService(typeof(T)) as Form;
        if (form != null)
        {
            form.MdiParent = this;
            form.WindowState = FormWindowState.Maximized;
            form.FormClosed += (s, e) => lblStatus.Text = "Listo";
            form.Show();
            lblStatus.Text = form.Text;
        }
    }

    private void OnAuthChanged()
    {
        if (!_authStore.IsAuthenticated)
            Close();
    }

    public void UpdateUIForUser()
    {
        Text = $"Devti POS Lite - {_authStore.CurrentUser?.Name}";
    }

    private void Logout()
    {
        _authStore.Logout();
        Close();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _authStore.OnChange -= OnAuthChanged;
        base.OnFormClosed(e);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.F2: OpenForm<PosForm>(); return true;
            case Keys.F3: OpenForm<ProductsForm>(); return true;
            case Keys.F4: OpenForm<CategoriesForm>(); return true;
            case Keys.F5: OpenForm<DenominationsForm>(); return true;
            case Keys.F6: OpenForm<ReportsForm>(); return true;
            case Keys.F7: OpenForm<CashoutForm>(); return true;
            case Keys.F8: OpenForm<ConfigForm>(); return true;
        }
        if (keyData == Keys.Escape && ActiveMdiChild != null)
        {
            ActiveMdiChild.Close();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
