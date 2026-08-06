using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.Core.Models;
using DevtiPosLite.UI.Helpers;

namespace DevtiPosLite.UI.Forms;

public partial class CategoriesForm : Form
{
    private readonly ICatalogService _catalogService;
    private List<Category> _categories = new();

    public CategoriesForm(ICatalogService catalogService)
    {
        _catalogService = catalogService;
        InitializeComponent();
    }

    private async void InitializeComponent()
    {
        UITheme.StyleForm(this, "Categorías", new Size(600, 400));

        var toolbar = new ToolStrip();
        toolbar.Items.Add(UITheme.CreateToolbarButton("Nueva", ButtonStyle.Primary, async (s, e) => await NewCategory()));
        toolbar.Items.Add(UITheme.CreateToolbarButton("Editar", ButtonStyle.Secondary, async (s, e) => await EditCategory()));
        toolbar.Items.Add(UITheme.CreateToolbarButton("Eliminar", ButtonStyle.Danger, async (s, e) => await DeleteCategory()));
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
        Controls.Add(dgv);

        dgv.CellDoubleClick += async (s, e) => await EditCategory();
        Load += async (s, e) => await LoadData();
    }

    private DataGridView dgv = null!;

    private async Task LoadData()
    {
        _categories = (await _catalogService.GetCategoriesAsync()).ToList();
        dgv.Rows.Clear();
        foreach (var c in _categories)
            dgv.Rows.Add(c.Id, c.Name);
    }

    private Category? GetSelected()
    {
        if (dgv.CurrentRow == null || dgv.CurrentRow.Index >= _categories.Count) return null;
        return _categories[dgv.CurrentRow.Index];
    }

    private async Task NewCategory()
    {
        var name = Microsoft.VisualBasic.Interaction.InputBox("Nombre de la categoría:", "Nueva Categoría", "");
        if (string.IsNullOrWhiteSpace(name)) return;
        await _catalogService.CreateCategoryAsync(new Category { Name = name.Trim() });
        await LoadData();
    }

    private async Task EditCategory()
    {
        var cat = GetSelected();
        if (cat == null) return;
        var name = Microsoft.VisualBasic.Interaction.InputBox("Nombre de la categoría:", "Editar Categoría", cat.Name);
        if (string.IsNullOrWhiteSpace(name)) return;
        cat.Name = name.Trim();
        await _catalogService.UpdateCategoryAsync(cat);
        await LoadData();
    }

    private async Task DeleteCategory()
    {
        var cat = GetSelected();
        if (cat == null) return;
        if (MessageBox.Show($"¿Eliminar '{cat.Name}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            await _catalogService.DeleteCategoryAsync(cat.Id);
            await LoadData();
        }
    }
}
