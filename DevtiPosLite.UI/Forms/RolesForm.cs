using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.Core.Models;
using DevtiPosLite.UI.Dialogs;
using DevtiPosLite.UI.Helpers;

namespace DevtiPosLite.UI.Forms;

public partial class RolesForm : Form
{
    private readonly IAdminService _adminService;
    private List<Role> _roles = new();
    private List<Permission> _permissions = new();

    public RolesForm(IAdminService adminService)
    {
        _adminService = adminService;
        InitializeComponent();
    }

    private async void InitializeComponent()
    {
        UITheme.StyleForm(this, "Roles", new Size(800, 450));

        var toolbar = new ToolStrip();
        toolbar.Items.Add(UITheme.CreateToolbarButton("Nuevo", ButtonStyle.Primary, async (s, e) => await ShowEditDialog(null)));
        toolbar.Items.Add(UITheme.CreateToolbarButton("Editar", ButtonStyle.Secondary, async (s, e) => await ShowEditDialog(GetSelected())));
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
        dgv.Columns.Add("Description", "Descripción");
        Controls.Add(dgv);

        dgv.CellDoubleClick += async (s, e) => await ShowEditDialog(GetSelected());
        Load += async (s, e) => await LoadData();
    }

    private DataGridView dgv = null!;

    private async Task LoadData()
    {
        _roles = (await _adminService.GetRolesAsync()).ToList();
        _permissions = (await _adminService.GetPermissionsAsync()).ToList();
        dgv.Rows.Clear();
        foreach (var r in _roles)
            dgv.Rows.Add(r.Id, r.Name, r.Description);
    }

    private Role? GetSelected()
    {
        if (dgv.CurrentRow == null || dgv.CurrentRow.Index >= _roles.Count) return null;
        return _roles[dgv.CurrentRow.Index];
    }

    private async Task ShowEditDialog(Role? role)
    {
        _permissions = (await _adminService.GetPermissionsAsync()).ToList();
        List<uint> selectedIds = new();
        if (role != null)
            selectedIds = (await _adminService.GetRolePermissionIdsAsync(role.Id)).ToList();

        var dialog = new RoleEditDialog(role, _permissions, selectedIds);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            if (role == null)
                role = await _adminService.CreateRoleAsync(dialog.Role);

            await _adminService.AssignPermissionsToRoleAsync(role.Id, dialog.SelectedPermissionIds);
            await LoadData();
        }
    }
}
