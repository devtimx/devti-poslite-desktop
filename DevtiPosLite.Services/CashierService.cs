using DevtiPosLite.Core.DTOs;
using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.Core.Models;

namespace DevtiPosLite.Services;

public class CashierService : ICashierService
{
    private readonly IRepository<CashOpening> _cashOpeningRepo;
    private readonly IRepository<CashoutHistory> _cashoutHistoryRepo;

    public CashierService(
        IRepository<CashOpening> cashOpeningRepo,
        IRepository<CashoutHistory> cashoutHistoryRepo)
    {
        _cashOpeningRepo = cashOpeningRepo;
        _cashoutHistoryRepo = cashoutHistoryRepo;
    }

    public async Task<CashOpening?> GetCurrentCashOpeningAsync(uint userId)
    {
        var openings = await _cashOpeningRepo.FindAsync(co =>
            co.UserId == userId && co.Status == "OPEN");
        return openings.FirstOrDefault();
    }

    public async Task<CashOpening> OpenCashAsync(CashOpenRequest request, uint userId)
    {
        var existing = await GetCurrentCashOpeningAsync(userId);
        if (existing != null)
            throw new InvalidOperationException("Ya tiene una caja abierta");

        var opening = new CashOpening
        {
            UserId = userId,
            OpeningAmount = request.OpeningAmount,
            Status = "OPEN",
            Notes = request.Notes ?? string.Empty
        };

        var result = await _cashOpeningRepo.AddAsync(opening);
        await _cashOpeningRepo.SaveChangesAsync();
        return result;
    }

    public async Task<CashoutHistory> CloseCashAsync(CashCloseRequest request, uint userId)
    {
        var opening = await GetCurrentCashOpeningAsync(userId)
            ?? throw new InvalidOperationException("No tiene caja abierta");

        opening.Status = "CLOSED";
        opening.ClosingAmount = request.ClosingAmount;
        opening.Notes = request.Notes ?? string.Empty;
        await _cashOpeningRepo.UpdateAsync(opening);

        var discrepancy = request.ClosingAmount - opening.OpeningAmount;

        var history = new CashoutHistory
        {
            CashOpeningId = opening.Id,
            TotalSales = 0,
            TotalCash = request.ClosingAmount,
            DiscrepancyAmount = discrepancy,
            Notes = request.Notes ?? string.Empty
        };

        var result = await _cashoutHistoryRepo.AddAsync(history);
        await _cashoutHistoryRepo.SaveChangesAsync();
        return result;
    }

    public async Task<IEnumerable<CashOpening>> GetCashOpeningHistoryAsync(
        uint? userId = null, DateTime? from = null, DateTime? to = null)
    {
        var all = await _cashOpeningRepo.GetAllAsync();
        var query = all.AsEnumerable();

        if (userId.HasValue)
            query = query.Where(co => co.UserId == userId.Value);
        if (from.HasValue)
            query = query.Where(co => co.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(co => co.CreatedAt <= to.Value);

        return query.OrderByDescending(co => co.CreatedAt);
    }

    public async Task<CashoutReportResponse> GetCashoutReportAsync(
        uint? userId = null, DateTime? from = null, DateTime? to = null)
    {
        var cashouts = await _cashoutHistoryRepo.GetAllAsync();
        var query = cashouts.AsEnumerable();

        if (userId.HasValue)
            query = query.Where(ch => ch.CashOpening.UserId == userId.Value);
        if (from.HasValue)
            query = query.Where(ch => ch.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(ch => ch.CreatedAt <= to.Value);

        var list = query.OrderByDescending(ch => ch.CreatedAt).ToList();

        return new CashoutReportResponse
        {
            Cashouts = list,
            TotalSales = list.Sum(ch => ch.TotalSales),
            TotalCash = list.Sum(ch => ch.TotalCash)
        };
    }
}
