using DevtiPosLite.UI.Helpers;

namespace DevtiPosLite.UI.Dialogs;

public partial class CashOpeningDialog : Form
{
    public decimal OpeningAmount { get; private set; }

    public CashOpeningDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        UITheme.StyleForm(this, "Apertura de Caja", new Size(360, 200));
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var lbl = new Label { Text = "Monto de apertura:", Location = new Point(20, 30), Size = new Size(150, 25), ForeColor = UITheme.TextMuted };
        nudAmount = new NumericUpDown { Location = new Point(180, 28), Size = new Size(130, 25), DecimalPlaces = 2, Maximum = 999999, Minimum = 0 };
        var btnOk = new Button { Text = "Abrir Caja", Location = new Point(60, 100), Size = new Size(110, 38), DialogResult = DialogResult.OK };
        UITheme.StyleButton(btnOk, ButtonStyle.Success);
        btnOk.Click += (s, e) => OpeningAmount = nudAmount.Value;
        var btnCancel = new Button { Text = "Cancelar", Location = new Point(190, 100), Size = new Size(110, 38), DialogResult = DialogResult.Cancel };
        UITheme.StyleGhostButton(btnCancel);

        Controls.Add(lbl);
        Controls.Add(nudAmount);
        Controls.Add(btnOk);
        Controls.Add(btnCancel);
        AcceptButton = btnOk;
    }

    private NumericUpDown nudAmount = null!;
}
