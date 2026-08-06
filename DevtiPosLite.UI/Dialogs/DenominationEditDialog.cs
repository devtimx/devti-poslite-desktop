using DevtiPosLite.Core.Models;
using DevtiPosLite.UI.Helpers;

namespace DevtiPosLite.UI.Dialogs;

public partial class DenominationEditDialog : Form
{
    public Denomination Denomination { get; private set; } = new();
    private string? _selectedImagePath;

    public DenominationEditDialog(Denomination? denom = null)
    {
        if (denom != null) Denomination = denom;
        InitializeComponent();
        if (denom != null)
        {
            cmbType.SelectedItem = denom.Type;
            nudValue.Value = denom.Value;
            if (!string.IsNullOrEmpty(denom.Image))
                ShowPreview(denom.Image);
        }
    }

    private void InitializeComponent()
    {
        UITheme.StyleForm(this, "Denominación", new Size(380, 340));
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 4, Padding = new Padding(10) };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        tbl.Controls.Add(new Label { Text = "Tipo:", TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Right, ForeColor = UITheme.TextMuted }, 0, 0);
        cmbType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
        cmbType.Items.AddRange(new[] { "BILLETE", "MONEDA" });
        tbl.Controls.Add(cmbType, 1, 0);
        tbl.SetColumnSpan(cmbType, 2);

        tbl.Controls.Add(new Label { Text = "Valor:", TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Right, ForeColor = UITheme.TextMuted }, 0, 1);
        nudValue = new NumericUpDown { DecimalPlaces = 2, Maximum = 999999, Anchor = AnchorStyles.Left | AnchorStyles.Right };
        tbl.Controls.Add(nudValue, 1, 1);
        tbl.SetColumnSpan(nudValue, 2);

        tbl.Controls.Add(new Label { Text = "Imagen:", TextAlign = ContentAlignment.MiddleRight, Anchor = AnchorStyles.Right, ForeColor = UITheme.TextMuted }, 0, 2);
        txtImage = new TextBox { ReadOnly = true, Anchor = AnchorStyles.Left | AnchorStyles.Right };
        tbl.Controls.Add(txtImage, 1, 2);
        var btnBrowse = new Button { Text = "Examinar", Anchor = AnchorStyles.Left | AnchorStyles.Right };
        UITheme.StyleGhostButton(btnBrowse);
        btnBrowse.Click += BrowseImage;
        tbl.Controls.Add(btnBrowse, 2, 2);

        picPreview = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle, Anchor = AnchorStyles.None, BackColor = UITheme.Surface };
        tbl.Controls.Add(picPreview, 1, 3);
        tbl.SetColumnSpan(picPreview, 2);

        var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, FlowDirection = FlowDirection.RightToLeft };
        var btnOk = new Button { Text = "Guardar", DialogResult = DialogResult.OK, Width = 110, Height = 34 };
        UITheme.StyleButton(btnOk, ButtonStyle.Primary);
        btnOk.Click += (s, e) => Save();
        var btnCancel = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, Width = 110, Height = 34 };
        UITheme.StyleGhostButton(btnCancel);
        btnPanel.Controls.Add(btnCancel);
        btnPanel.Controls.Add(btnOk);

        Controls.Add(tbl);
        Controls.Add(btnPanel);
        AcceptButton = btnOk;
        Load += (s, e) => { if (cmbType.Items.Count > 0) cmbType.SelectedIndex = 0; };
    }

    private ComboBox cmbType = null!;
    private NumericUpDown nudValue = null!;
    private TextBox txtImage = null!;
    private PictureBox picPreview = null!;

    private void BrowseImage(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog();
        ofd.Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
        ofd.Title = "Seleccionar imagen";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            _selectedImagePath = ofd.FileName;
            txtImage.Text = ofd.FileName;
            try { picPreview.Image = Image.FromFile(ofd.FileName); }
            catch { picPreview.Image = null; }
        }
    }

    private void ShowPreview(string relativePath)
    {
        txtImage.Text = relativePath;
        picPreview.Image = ImageHelper.LoadImage(relativePath);
    }

    private void Save()
    {
        Denomination.Type = cmbType.SelectedItem?.ToString() ?? "BILLETE";
        Denomination.Value = nudValue.Value;
        if (_selectedImagePath != null)
        {
            var oldImage = Denomination.Image;
            Denomination.Image = ImageHelper.SaveImage(_selectedImagePath, "denominations");
            if (!string.IsNullOrEmpty(oldImage) && oldImage != Denomination.Image)
                ImageHelper.DeleteImage(oldImage);
        }
    }
}
