using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DevtiPosLite.Services;

public class CatalogService : ICatalogService
{
    private readonly IRepository<Category> _categoryRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<Denomination> _denominationRepo;

    public CatalogService(
        IRepository<Category> categoryRepo,
        IRepository<Product> productRepo,
        IRepository<Denomination> denominationRepo)
    {
        _categoryRepo = categoryRepo;
        _productRepo = productRepo;
        _denominationRepo = denominationRepo;
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
        => await _categoryRepo.GetAllAsync();

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        var result = await _categoryRepo.AddAsync(category);
        await _categoryRepo.SaveChangesAsync();
        return result;
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        await _categoryRepo.UpdateAsync(category);
        await _categoryRepo.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(uint id)
    {
        var cat = await _categoryRepo.GetByIdAsync(id);
        if (cat != null)
        {
            await _categoryRepo.DeleteAsync(cat);
            await _categoryRepo.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Product>> GetProductsAsync(string? search = null)
    {
        if (string.IsNullOrWhiteSpace(search))
            return await _productRepo.GetAllAsync();

        var products = await _productRepo.FindAsync(p =>
            p.Name.Contains(search) || p.Barcode.Contains(search));
        return products;
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        var result = await _productRepo.AddAsync(product);
        await _productRepo.SaveChangesAsync();
        return result;
    }

    public async Task UpdateProductAsync(Product product)
    {
        await _productRepo.UpdateAsync(product);
        await _productRepo.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(uint id)
    {
        var prod = await _productRepo.GetByIdAsync(id);
        if (prod != null)
        {
            await _productRepo.DeleteAsync(prod);
            await _productRepo.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Denomination>> GetDenominationsAsync()
        => await _denominationRepo.GetAllAsync();

    public async Task<Denomination> CreateDenominationAsync(Denomination denomination)
    {
        var result = await _denominationRepo.AddAsync(denomination);
        await _denominationRepo.SaveChangesAsync();
        return result;
    }

    public async Task UpdateDenominationAsync(Denomination denomination)
    {
        await _denominationRepo.UpdateAsync(denomination);
        await _denominationRepo.SaveChangesAsync();
    }

    public async Task DeleteDenominationAsync(uint id)
    {
        var denom = await _denominationRepo.GetByIdAsync(id);
        if (denom != null)
        {
            await _denominationRepo.DeleteAsync(denom);
            await _denominationRepo.SaveChangesAsync();
        }
    }
}
