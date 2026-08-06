namespace DevtiPosLite.UI.Helpers;

public enum ButtonStyle { Primary, Secondary, Success, Danger, Warning, Ghost }

public static class UITheme
{
    public static readonly Color Primary = Color.FromArgb(37, 99, 235);
    public static readonly Color PrimaryDark = Color.FromArgb(29, 78, 216);
    public static readonly Color Secondary = Color.FromArgb(59, 130, 246);
    public static readonly Color Background = Color.FromArgb(248, 250, 252);
    public static readonly Color Surface = Color.White;
    public static readonly Color Text = Color.FromArgb(30, 41, 59);
    public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);
    public static readonly Color Border = Color.FromArgb(226, 232, 240);
    public static readonly Color Success = Color.FromArgb(16, 185, 129);
    public static readonly Color SuccessDark = Color.FromArgb(5, 150, 105);
    public static readonly Color Danger = Color.FromArgb(220, 38, 38);
    public static readonly Color DangerDark = Color.FromArgb(185, 28, 28);
    public static readonly Color Warning = Color.FromArgb(245, 158, 11);
    public static readonly Color Accent = Color.FromArgb(124, 58, 237);
    public static readonly Color HeaderRow = Color.FromArgb(241, 245, 249);

    public static readonly Font FontNormal = new Font("Segoe UI", 10);
    public static readonly Font FontBold = new Font("Segoe UI", 10, FontStyle.Bold);
    public static readonly Font FontTitle = new Font("Segoe UI", 18, FontStyle.Bold);
    public static readonly Font FontSubtitle = new Font("Segoe UI", 11);
    public static readonly Font FontSmall = new Font("Segoe UI", 9);
    public static readonly Font FontSmallBold = new Font("Segoe UI", 9, FontStyle.Bold);

    public static void StyleForm(Form form, string title, Size size)
    {
        form.Text = title;
        form.Size = size;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.BackColor = Background;
        form.Font = FontNormal;
        form.ForeColor = Text;
    }

    public static Button StyleButton(Button btn, ButtonStyle style = ButtonStyle.Primary)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.Font = FontBold;
        btn.FlatAppearance.BorderSize = 0;
        btn.Cursor = Cursors.Hand;
        btn.BackColor = ColorOf(style);
        btn.ForeColor = Color.White;
        return btn;
    }

    public static Button StyleGhostButton(Button btn)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.Font = FontNormal;
        btn.FlatAppearance.BorderSize = 1;
        btn.FlatAppearance.BorderColor = Border;
        btn.BackColor = Surface;
        btn.ForeColor = Text;
        btn.Cursor = Cursors.Hand;
        return btn;
    }

    public static ToolStripButton CreateToolbarButton(string text, ButtonStyle style, EventHandler onClick)
    {
        var btn = new ToolStripButton(text) { Font = FontBold, Padding = new Padding(8, 3, 8, 3) };
        btn.Click += onClick;
        btn.BackColor = Color.Transparent;
        btn.ForeColor = style switch
        {
            ButtonStyle.Danger => Danger,
            ButtonStyle.Success => Success,
            ButtonStyle.Ghost => Text,
            ButtonStyle.Warning => Warning,
            _ => Primary
        };
        return btn;
    }

    public static void StyleToolbar(ToolStrip strip, params ToolStripItem[] extraItems)
    {
        StyleToolStrip(strip);
        if (extraItems.Length > 0)
            strip.Items.AddRange(extraItems);
    }

    private static Color ColorOf(ButtonStyle style) => style switch
    {
        ButtonStyle.Primary => Primary,
        ButtonStyle.Secondary => Secondary,
        ButtonStyle.Success => Success,
        ButtonStyle.Danger => Danger,
        ButtonStyle.Warning => Warning,
        _ => Primary
    };

    public static void StyleDataGrid(DataGridView dgv)
    {
        dgv.BackgroundColor = Surface;
        dgv.BorderStyle = BorderStyle.FixedSingle;
        dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = HeaderRow;
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Text;
        dgv.ColumnHeadersDefaultCellStyle.Font = FontSmallBold;
        dgv.ColumnHeadersHeight = 34;
        dgv.DefaultCellStyle.Font = FontNormal;
        dgv.DefaultCellStyle.ForeColor = Text;
        dgv.DefaultCellStyle.SelectionBackColor = Primary;
        dgv.DefaultCellStyle.SelectionForeColor = Color.White;
        dgv.RowTemplate.Height = 32;
        dgv.GridColor = Border;
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgv.AllowUserToResizeRows = false;
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
    }

    public static void StyleToolStrip(ToolStrip strip)
    {
        strip.Renderer = new ToolStripProfessionalRenderer(new ThemeColorTable());
        strip.BackColor = Surface;
        strip.GripStyle = ToolStripGripStyle.Hidden;
        strip.Padding = new Padding(6, 4, 6, 4);
    }

    public static void StyleMenu(MenuStrip menu)
    {
        menu.Renderer = new ToolStripProfessionalRenderer(new ThemeColorTable());
        menu.BackColor = Surface;
        menu.ForeColor = Text;
        menu.Padding = new Padding(6, 2, 6, 2);
        foreach (ToolStripItem item in menu.Items)
        {
            item.ForeColor = Text;
            if (item is ToolStripMenuItem mi)
            {
                mi.Font = FontNormal;
                StyleDropDown(mi.DropDown);
            }
        }
    }

    private static void StyleDropDown(ToolStripDropDown dd)
    {
        dd.Renderer = new ToolStripProfessionalRenderer(new ThemeColorTable());
        dd.BackColor = Surface;
        foreach (ToolStripItem item in dd.Items)
        {
            if (item is ToolStripMenuItem child)
            {
                child.ForeColor = Text;
                child.Font = FontNormal;
                StyleDropDown(child.DropDown);
            }
        }
    }

    public static void StyleTextBox(TextBox tb)
    {
        tb.BorderStyle = BorderStyle.FixedSingle;
        tb.Font = FontNormal;
        tb.ForeColor = Text;
    }

    public static void StyleGroupBox(GroupBox gb)
    {
        gb.ForeColor = PrimaryDark;
        gb.Font = FontSmallBold;
        gb.BackColor = Surface;
    }

    public static Panel CreateCard()
    {
        return new Panel
        {
            BackColor = Surface,
            Padding = new Padding(12),
            Margin = new Padding(6)
        };
    }

    public static Label CreateTitle(string text) => new Label
    {
        Text = text,
        Font = FontTitle,
        ForeColor = Text,
        AutoSize = true
    };

    public static Label CreateMuted(string text) => new Label
    {
        Text = text,
        Font = FontNormal,
        ForeColor = TextMuted,
        AutoSize = true
    };

    public static void KeepSplitRatio(SplitContainer split, double leftRatio)
    {
        if (split.Width <= 200) return;
        var max = split.Width - split.SplitterWidth - split.Panel2MinSize;
        var dist = (int)(split.Width * leftRatio);
        split.SplitterDistance = Math.Clamp(dist, split.Panel1MinSize, Math.Max(split.Panel1MinSize, max));
    }
}

public class ThemeColorTable : ProfessionalColorTable
{
    private static readonly Color LightBlue = Color.FromArgb(219, 234, 254);
    private static readonly Color MidBlue = Color.FromArgb(191, 219, 254);

    public override Color ToolStripDropDownBackground => UITheme.Surface;
    public override Color ImageMarginGradientBegin => UITheme.Surface;
    public override Color ImageMarginGradientMiddle => UITheme.Surface;
    public override Color ImageMarginGradientEnd => UITheme.Surface;
    public override Color MenuItemSelected => LightBlue;
    public override Color MenuItemBorder => MidBlue;
    public override Color MenuBorder => UITheme.Border;
    public override Color MenuItemSelectedGradientBegin => LightBlue;
    public override Color MenuItemSelectedGradientEnd => LightBlue;
    public override Color MenuItemPressedGradientBegin => MidBlue;
    public override Color MenuItemPressedGradientMiddle => MidBlue;
    public override Color MenuItemPressedGradientEnd => MidBlue;
    public override Color ToolStripGradientBegin => UITheme.Surface;
    public override Color ToolStripGradientMiddle => UITheme.Surface;
    public override Color ToolStripGradientEnd => UITheme.Surface;
    public override Color ToolStripBorder => UITheme.Border;
}
