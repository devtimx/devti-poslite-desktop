using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.Core.Models;
using DevtiPosLite.UI.Dialogs;
using DevtiPosLite.UI.Helpers;

namespace DevtiPosLite.UI.Forms;

public partial class UsersForm : Form
{
    private readonly IAdminService _adminService;
    private List<User> _users = new();
    private List<Role> _roles = new();

    public UsersForm(IAdminService adminService)
    {
        _adminService = adminService;
        InitializeComponent();
    }

    private async void InitializeComponent()
    {
        UITheme.StyleForm(this, "Usuarios", new Size(900, 450));

        var toolbar = new ToolStrip();
        toolbar.Items.Add(UITheme.CreateToolbarButton("Nuevo", ButtonStyle.Primary, async (s, e) => await ShowEditDialog(null)));
        toolbar.Items.Add(UITheme.CreateToolbarButton("Editar", ButtonStyle.Secondary, async (s, e) => await ShowEditDialog(GetSelected())));
        toolbar.Items.Add(UITheme.CreateToolbarButton("Eliminar", ButtonStyle.Danger, async (s, e) => await DeleteSelected()));
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
        dgv.Columns.Add("Name", "Nombre");
        dgv.Columns.Add("Email", "Email");
        dgv.Columns.Add("Role", "Rol");
        dgv.Columns.Add("Status", "Estado");
        Controls.Add(dgv);

        dgv.CellDoubleClick += async (s, e) => await ShowEditDialog(GetSelected());
        Load += async (s, e) => await LoadData();
    }

    private DataGridView dgv = null!;

    private async Task LoadData()
    {
        _users = (await _adminService.GetUsersAsync()).ToList();
        _roles = (await _adminService.GetRolesAsync()).ToList();
        dgv.Rows.Clear();
        foreach (var u in _users)
        {
            var roleName = _roles.FirstOrDefault(r => r.Id == u.RoleId)?.Name ?? "";
            dgv.Rows.Add(u.Id, u.Name, u.Email, roleName, u.Status);
        }
    }

    private User? GetSelected()
    {
        if (dgv.CurrentRow == null || dgv.CurrentRow.Index >= _users.Count) return null;
        return _users[dgv.CurrentRow.Index];
    }

    private async Task ShowEditDialog(User? user)
    {
        _roles = (await _adminService.GetRolesAsync()).ToList();
        var dialog = new UserEditDialog(user, _roles);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            if (user == null)
            {
                if (string.IsNullOrWhiteSpace(dialog.Password))
                {
                    MessageBox.Show("Contraseña requerida para nuevo usuario", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                await _adminService.CreateUserAsync(dialog.User, dialog.Password);
            }
            else
            {
                await _adminService.UpdateUserAsync(dialog.User);
            }
            await LoadData();
        }
    }

    private async Task DeleteSelected()
    {
        var u = GetSelected();
        if (u == null) return;
        if (MessageBox.Show($"¿Eliminar {u.Name}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            await _adminService.DeleteUserAsync(u.Id);
            await LoadData();
        }
    }
}
