using DevtiPosLite.Core.DTOs;
using DevtiPosLite.Core.Models;

namespace DevtiPosLite.Core.Interfaces;

public interface ICashierService
{
    Task<CashOpening?> GetCurrentCashOpeningAsync(uint userId);
    Task<CashOpening> OpenCashAsync(CashOpenRequest request, uint userId);
    Task<CashoutHistory> CloseCashAsync(CashCloseRequest request, uint userId);
    Task<IEnumerable<CashOpening>> GetCashOpeningHistoryAsync(uint? userId = null, DateTime? from = null, DateTime? to = null);
    Task<CashoutReportResponse> GetCashoutReportAsync(uint? userId = null, DateTime? from = null, DateTime? to = null);
}
