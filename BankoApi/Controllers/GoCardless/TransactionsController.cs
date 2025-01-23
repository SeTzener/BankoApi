using BankoApi.Data;
using BankoApi.Repository;
using BankoApi.Services;
using BankoApi.Services.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Controllers.GoCardless;

[ApiController]
[Route("[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly BankoDbContext _dbContext;
    private readonly GoCardlessService _goCardlessService;
    private readonly TransactionsRepository _repository;

    public TransactionsController(GoCardlessService goCardlessService, BankoDbContext dbContext)
    {
        _goCardlessService = goCardlessService;
        _dbContext = dbContext;
        _repository = new TransactionsRepository();
    }

    [HttpGet("gocardless/{accountId}")]
    public async Task<IActionResult> FetchAndStoreTransactions(string accountId)
    {
        // TODO(): Handle the exceptions
        var transactions = await _goCardlessService.GetTransactionsAsync(accountId);
        
        if (transactions == null) return NotFound();
        _repository.StoreTransactions(_dbContext, transactions);
        
        await _dbContext.SaveChangesAsync();
        return Ok("Transactions stored successfully.");
    }

    [HttpGet]
    public async Task<PaginatedTransactionResponse> GetTransactions(
        int pageNumber = 1,
        int pageSize = 30,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        ValidateTransaction(pageNumber, pageSize, fromDate, toDate);
        toDate ??= DateTime.UtcNow;
        fromDate ??= _dbContext.Transactions.Min(t => t.BookingDate);

        var skip = (pageNumber - 1) * pageSize;

        // Query filtered by date range
        var query = _dbContext.Transactions.Where(t => t.BookingDate >= fromDate && t.BookingDate <= toDate);

        var totalCount = query.Count();

        var transactions = await _dbContext.Transactions
            .Include(t => t.DebtorAccount)
            .Where(t => t.BookingDate >= fromDate && t.BookingDate <= toDate)
            .OrderByDescending(t => t.BookingDate)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedTransactionResponse()
        {
            Transactions = transactions,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private void ValidateTransaction(int pageNumber, int pageSize, DateTime? fromDate, DateTime? toDate)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            throw new ArgumentException("Page number and page size must be greater than zero.");
        }

        if (fromDate != null)
        {
            toDate ??= DateTime.UtcNow;
            if (fromDate > toDate)
            {
                throw new ArgumentException("The fromDate cannot be greater than toDate.");
            }
        }
    }
}
