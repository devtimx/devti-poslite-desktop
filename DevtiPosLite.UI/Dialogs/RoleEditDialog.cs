using DevtiPosLite.Core.Models;
using DevtiPosLite.UI.Helpers;

namespace DevtiPosLite.UI.Dialogs;

public partial class RoleEditDialog : Form
{
    public Role Role { get; private set; } = new();
    public List<uint> SelectedPermissionIds { get; private set; } = new();

    public RoleEditDialog(Role? role = null, List<Permission>? permissions = null, List<uint>? selectedPermIds = null)
    {
        if (role != null) Role = role;
        InitializeComponent();
        if (permissions != null)
        {
            foreach (var p in permissions)
            {
                var idx = clbPermissions.Items.Add(p.Name, selectedPermIds?.Contains(p.Id) == true);
                clbPermissions.Tag = permissions;
            }
        }
        if (role != null) { txtName.Text = role.Name; txtDescription.Text = role.Description; }
    }

    private void InitializeComponent()
    {
        UITheme.StyleForm(this, "Rol", new Size(400, 450));
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var lblName = new Label { Text = "Nombre:", Location = new Point(10, 10), Size = new Size(80, 25), ForeColor = UITheme.TextMuted };
        txtName = new TextBox { Location = new Point(100, 10), Size = new Size(280, 25) };

        var lblDesc = new Label { Text = "Descripción:", Location = new Point(10, 45), Size = new Size(80, 25), ForeColor = UITheme.TextMuted };
        txtDescription = new TextBox { Location = new Point(100, 45), Size = new Size(280, 25) };

        var lblPerms = new Label { Text = "Permisos:", Location = new Point(10, 80), Size = new Size(100, 25), ForeColor = UITheme.TextMuted };
        clbPermissions = new CheckedListBox { Location = new Point(10, 110), Size = new Size(370, 260), CheckOnClick = true };

        var btnOk = new Button { Text = "Guardar", Location = new Point(100, 380), Size = new Size(100, 34), DialogResult = DialogResult.OK };
        UITheme.StyleButton(btnOk, ButtonStyle.Primary);
        btnOk.Click += (s, e) => SaveRole();
        var btnCancel = new Button { Text = "Cancelar", Location = new Point(220, 380), Size = new Size(100, 34), DialogResult = DialogResult.Cancel };
        UITheme.StyleGhostButton(btnCancel);

        Controls.Add(lblName); Controls.Add(txtName);
        Controls.Add(lblDesc); Controls.Add(txtDescription);
        Controls.Add(lblPerms); Controls.Add(clbPermissions);
        Controls.Add(btnOk); Controls.Add(btnCancel);
        AcceptButton = btnOk;
    }

    private TextBox txtName = null!, txtDescription = null!;
    private CheckedListBox clbPermissions = null!;

    private void SaveRole()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Nombre es requerido", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }
        Role.Name = txtName.Text.Trim();
        Role.Description = txtDescription.Text.Trim();
        SelectedPermissionIds = new List<uint>();
        if (clbPermissions.Tag is List<Permission> perms)
        {
            for (int i = 0; i < clbPermissions.Items.Count; i++)
                if (clbPermissions.GetItemChecked(i) && i < perms.Count)
                    SelectedPermissionIds.Add(perms[i].Id);
        }
    }
}
