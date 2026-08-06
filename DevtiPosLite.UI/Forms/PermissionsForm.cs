using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.UI.Helpers;

namespace DevtiPosLite.UI.Forms;

public partial class PermissionsForm : Form
{
    private readonly IAdminService _adminService;

    public PermissionsForm(IAdminService adminService)
    {
        _adminService = adminService;
        UITheme.StyleForm(this, "Permisos del Sistema", new Size(600, 400));

        dgv = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        UITheme.StyleDataGrid(dgv);
        dgv.Columns.Add("Id", "ID");
        dgv.Columns.Add("Name", "Nombre");
        Controls.Add(dgv);

        Load += async (s, e) => await LoadData();
    }

    private DataGridView dgv = null!;

    private async Task LoadData()
    {
        var perms = await _adminService.GetPermissionsAsync();
        dgv.Rows.Clear();
        foreach (var p in perms)
            dgv.Rows.Add(p.Id, p.Name);
    }
}
