using DevtiPosLite.Core.DTOs;
using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.Core.Models;
using DevtiPosLite.Data;
using Microsoft.EntityFrameworkCore;

namespace DevtiPosLite.Services;

public class SalesService : ISalesService
{
    private readonly IRepository<Sale> _saleRepo;
    private readonly IRepository<SaleDetail> _detailRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<Return> _returnRepo;
    private readonly IRepository<User> _userRepo;
    private readonly AppDbContext _context;

    public SalesService(
        IRepository<Sale> saleRepo,
        IRepository<SaleDetail> detailRepo,
        IRepository<Product> productRepo,
        IRepository<Return> returnRepo,
        IRepository<User> userRepo,
        AppDbContext context)
    {
        _saleRepo = saleRepo;
        _detailRepo = detailRepo;
        _productRepo = productRepo;
        _returnRepo = returnRepo;
        _userRepo = userRepo;
        _context = context;
    }

    public async Task<Sale> CreateSaleAsync(SaleRequest request, uint userId)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new ArgumentException("Debe agregar al menos un producto");

        var sale = new Sale
        {
            UserId = userId,
            Status = "PAID",
            Cash = request.Cash,
            Total = 0,
            Items = 0,
            Change = 0
        };

        decimal total = 0;
        int totalItems = 0;
        var details = new List<SaleDetail>();

        foreach (var item in request.Items)
        {
            var product = await _productRepo.GetByIdAsync(item.ProductId)
                ?? throw new ArgumentException($"Producto {item.ProductId} no encontrado");

            if (product.Stock < item.Quantity)
                throw new InvalidOperationException($"Stock insuficiente para {product.Name}");

            product.Stock -= item.Quantity;
            await _productRepo.UpdateAsync(product);

            var detail = new SaleDetail
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price,
                Sale = sale
            };
            details.Add(detail);

            total += item.Price * item.Quantity;
            totalItems += item.Quantity;
        }

        sale.Total = total;
        sale.Items = totalItems;
        sale.Change = sale.Cash - total;
        sale.Details = details;

        sale = await _saleRepo.AddAsync(sale);
        await _saleRepo.SaveChangesAsync();

        return sale;
    }

    public async Task<IEnumerable<Sale>> GetSalesAsync(uint? userId = null, DateTime? from = null, DateTime? to = null)
    {
        var query = await _saleRepo.GetAllAsync();
        var sales = query.AsEnumerable();

        if (userId.HasValue)
            sales = sales.Where(s => s.UserId == userId.Value);
        if (from.HasValue)
            sales = sales.Where(s => s.CreatedAt >= from.Value);
        if (to.HasValue)
            sales = sales.Where(s => s.CreatedAt < to.Value.AddDays(1));

        return sales.OrderByDescending(s => s.CreatedAt);
    }

    public async Task<Sale?> GetSaleByIdAsync(uint id)
        => await _saleRepo.GetByIdAsync(id);

    public async Task<Sale?> GetSaleWithDetailsAsync(uint id)
        => await _context.Sales.Include(s => s.Details).ThenInclude(d => d.Product).FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Return> CreateReturnAsync(ReturnRequest request, uint userId)
    {
        var sale = await _saleRepo.GetByIdAsync(request.SaleId)
            ?? throw new ArgumentException("Venta no encontrada");

        var product = await _productRepo.GetByIdAsync(request.ProductId)
            ?? throw new ArgumentException("Producto no encontrado");

        var detail = (await _detailRepo.FindAsync(d =>
            d.SaleId == request.SaleId && d.ProductId == request.ProductId)).FirstOrDefault()
            ?? throw new ArgumentException("Producto no encontrado en la venta");

        if (request.Quantity > detail.Quantity)
            throw new InvalidOperationException("Cantidad excede lo vendido");

        product.Stock += request.Quantity;
        await _productRepo.UpdateAsync(product);

        var refundAmount = product.Price * request.Quantity;

        var returnRecord = new Return
        {
            SaleId = request.SaleId,
            ProductId = request.ProductId,
            UserId = userId,
            Quantity = request.Quantity,
            RefundAmount = refundAmount,
            SaleReference = sale.Id.ToString()
        };

        returnRecord = await _returnRepo.AddAsync(returnRecord);
        await _returnRepo.SaveChangesAsync();

        return returnRecord;
    }

    public async Task<SalesReportResponse> GetSalesReportAsync(uint? userId = null, DateTime? from = null, DateTime? to = null)
    {
        var sales = await GetSalesAsync(userId, from, to);
        var list = sales.ToList();

        return new SalesReportResponse
        {
            Sales = list,
            TotalAmount = list.Sum(s => s.Total),
            TotalItems = list.Sum(s => s.Items)
        };
    }

    public async Task<List<SalesReportLineDto>> GetSalesDetailReportAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.SaleDetails
            .Include(d => d.Product)
            .Include(d => d.Sale).ThenInclude(s => s.User)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(d => d.Sale.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(d => d.Sale.CreatedAt < to.Value.AddDays(1));

        return await query
            .OrderByDescending(d => d.Sale.CreatedAt)
            .Select(d => new SalesReportLineDto
            {
                SaleId = d.SaleId,
                ProductName = d.Product.Name,
                Quantity = d.Quantity,
                Price = d.Price,
                UserName = d.Sale.User.Name,
                CreatedAt = d.Sale.CreatedAt
            })
            .ToListAsync();
    }
}
