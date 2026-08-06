using DevtiPosLite.Core.Models;
using DevtiPosLite.UI.Helpers;

namespace DevtiPosLite.UI.Dialogs;

public partial class UserEditDialog : Form
{
    public User User { get; private set; } = new();
    public string Password { get; private set; } = "";
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public List<Role> Roles { get; set; } = new();

    public UserEditDialog(User? user = null, List<Role>? roles = null)
    {
        if (user != null) User = user;
        if (roles != null) Roles = roles;
        InitializeComponent();
        if (user != null) LoadUser(user);
    }

    private void LoadUser(User u)
    {
        txtName.Text = u.Name;
        txtEmail.Text = u.Email;
        txtPhone.Text = u.Phone;
        cmbStatus.SelectedItem = u.Status;
        if (Roles.Count > 0 && u.RoleId.HasValue)
        {
            var idx = Roles.FindIndex(r => r.Id == u.RoleId.Value);
            if (idx >= 0) cmbRole.SelectedIndex = idx;
        }
    }

    private void InitializeComponent()
    {
        UITheme.StyleForm(this, "Usuario", new Size(400, 350));
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6, Padding = new Padding(10) };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

        AddRow(tbl, "Nombre:", txtName = new TextBox(), 0);
        AddRow(tbl, "Email:", txtEmail = new TextBox(), 1);
        AddRow(tbl, "Teléfono:", txtPhone = new TextBox(), 2);
        AddRow(tbl, "Contraseña:", txtPassword = new TextBox { UseSystemPasswordChar = true }, 3);

        tbl.Controls.Add(new Label { Text = "Rol:", TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Right, ForeColor = UITheme.TextMuted }, 0, 4);
        cmbRole = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
        cmbRole.Items.AddRange(Roles.Select(r => r.Name).ToArray());
        tbl.Controls.Add(cmbRole, 1, 4);

        tbl.Controls.Add(new Label { Text = "Estado:", TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Right, ForeColor = UITheme.TextMuted }, 0, 5);
        cmbStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
        cmbStatus.Items.AddRange(new[] { "ACTIVE", "INACTIVE" });
        tbl.Controls.Add(cmbStatus, 1, 5);

        var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, FlowDirection = FlowDirection.RightToLeft };
        var btnOk = new Button { Text = "Guardar", DialogResult = DialogResult.OK, Width = 110, Height = 34 };
        UITheme.StyleButton(btnOk, ButtonStyle.Primary);
        btnOk.Click += (s, e) => SaveUser();
        var btnCancel = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Width = 110, Height = 34 };
        UITheme.StyleGhostButton(btnCancel);
        btnPanel.Controls.Add(btnCancel);
        btnPanel.Controls.Add(btnOk);

        Controls.Add(tbl);
        Controls.Add(btnPanel);
        AcceptButton = btnOk;
        Load += (s, e) =>
        {
            cmbStatus.SelectedIndex = 0;
            if (cmbRole.Items.Count > 0) cmbRole.SelectedIndex = 0;
        };
    }

    private TextBox txtName = null!, txtEmail = null!, txtPhone = null!, txtPassword = null!;
    private ComboBox cmbRole = null!, cmbStatus = null!;

    private void AddRow(TableLayoutPanel tbl, string label, Control ctrl, int row)
    {
        tbl.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Right, ForeColor = UITheme.TextMuted }, 0, row);
        ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        tbl.Controls.Add(ctrl, 1, row);
    }

    private void SaveUser()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text))
        {
            MessageBox.Show("Nombre y email son requeridos", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }
        User.Name = txtName.Text.Trim();
        User.Email = txtEmail.Text.Trim();
        User.Phone = txtPhone.Text.Trim();
        Password = txtPassword.Text;
        User.Status = cmbStatus.SelectedItem?.ToString() ?? "ACTIVE";
        if (cmbRole.SelectedIndex >= 0)
            User.RoleId = Roles[cmbRole.SelectedIndex].Id;
    }
}
