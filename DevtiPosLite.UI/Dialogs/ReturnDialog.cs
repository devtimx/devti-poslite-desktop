using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.UI.Helpers;

namespace DevtiPosLite.UI.Dialogs;

public partial class ReturnDialog : Form
{
    private readonly ISalesService _salesService;

    public uint? ReturnSaleId { get; private set; }
    public uint? ReturnProductId { get; private set; }
    public int ReturnQuantity { get; private set; }

    public ReturnDialog(ISalesService salesService)
    {
        _salesService = salesService;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        UITheme.StyleForm(this, "Devolución de Producto", new Size(400, 200));
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var lblSale = new Label { Text = "ID Venta:", Location = new Point(20, 30), Size = new Size(80, 25), ForeColor = UITheme.TextMuted };
        txtSaleId = new TextBox { Location = new Point(110, 28), Size = new Size(100, 25) };
        var lblProduct = new Label { Text = "ID Producto:", Location = new Point(20, 65), Size = new Size(80, 25), ForeColor = UITheme.TextMuted };
        txtProductId = new TextBox { Location = new Point(110, 63), Size = new Size(100, 25) };
        var lblQty = new Label { Text = "Cantidad:", Location = new Point(20, 100), Size = new Size(80, 25), ForeColor = UITheme.TextMuted };
        nudQuantity = new NumericUpDown { Location = new Point(110, 98), Size = new Size(100, 25), Minimum = 1, Maximum = 999 };

        var btnOk = new Button { Text = "Devolver", Location = new Point(60, 140), Size = new Size(110, 34), DialogResult = DialogResult.OK };
        UITheme.StyleButton(btnOk, ButtonStyle.Danger);
        btnOk.Click += (s, e) =>
        {
            if (uint.TryParse(txtSaleId.Text, out var saleId) && uint.TryParse(txtProductId.Text, out var prodId))
            {
                ReturnSaleId = saleId;
                ReturnProductId = prodId;
                ReturnQuantity = (int)nudQuantity.Value;
            }
            else
            {
                MessageBox.Show("IDs inválidos", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
            }
        };
        var btnCancel = new Button { Text = "Cancelar", Location = new Point(190, 140), Size = new Size(110, 34), DialogResult = DialogResult.Cancel };
        UITheme.StyleGhostButton(btnCancel);

        Controls.Add(lblSale); Controls.Add(txtSaleId);
        Controls.Add(lblProduct); Controls.Add(txtProductId);
        Controls.Add(lblQty); Controls.Add(nudQuantity);
        Controls.Add(btnOk); Controls.Add(btnCancel);
        AcceptButton = btnOk;
    }

    private TextBox txtSaleId = null!;
    private TextBox txtProductId = null!;
    private NumericUpDown nudQuantity = null!;
}
