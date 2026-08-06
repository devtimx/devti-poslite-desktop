using DevtiPosLite.Core.Models;

namespace DevtiPosLite.Core.Interfaces;

public interface ICatalogService
{
    Task<IEnumerable<Category>> GetCategoriesAsync();
    Task<Category> CreateCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(uint id);

    Task<IEnumerable<Product>> GetProductsAsync(string? search = null);
    Task<Product> CreateProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(uint id);

    Task<IEnumerable<Denomination>> GetDenominationsAsync();
    Task<Denomination> CreateDenominationAsync(Denomination denomination);
    Task UpdateDenominationAsync(Denomination denomination);
    Task DeleteDenominationAsync(uint id);
}
