using DevtiPosLite.Core.DTOs;
using DevtiPosLite.Core.Models;

namespace DevtiPosLite.Core.Interfaces;

public interface ISalesService
{
    Task<Sale> CreateSaleAsync(SaleRequest request, uint userId);
    Task<IEnumerable<Sale>> GetSalesAsync(uint? userId = null, DateTime? from = null, DateTime? to = null);
    Task<Sale?> GetSaleByIdAsync(uint id);
    Task<Return> CreateReturnAsync(ReturnRequest request, uint userId);
    Task<SalesReportResponse> GetSalesReportAsync(uint? userId = null, DateTime? from = null, DateTime? to = null);
    Task<List<SalesReportLineDto>> GetSalesDetailReportAsync(DateTime? from = null, DateTime? to = null);
    Task<Sale?> GetSaleWithDetailsAsync(uint id);
}
