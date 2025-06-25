using BankoApi.Controllers.BankoApi.Requests;
using BankoApi.Data;
using BankoApi.Services.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Controllers.BankoApi;

[ApiController]
[Route("[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly BankoDbContext _dbContext;

    public TransactionsController(BankoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions(
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

        // First get just the IDs with pagination
        var transactionIds = await _dbContext.Transactions
            .Where(t => t.BookingDate >= fromDate && t.BookingDate <= toDate)
            .OrderByDescending(t => t.BookingDate)
            .Skip(skip)
            .Take(pageSize)
            .Select(t => t.Id)
            .ToListAsync();

// Then load full entities with relationships
        var transactions = await _dbContext.Transactions
            .Where(t => transactionIds.Contains(t.Id))
            .Include(t => t.DebtorAccount)
            .Include(t => t.CreditorAccount)
            .Include(t => t.ExpenseTag)
            .OrderByDescending(t => t.BookingDate)
            .ToListAsync();

        return Ok(new PaginatedTransactionResponse
        {
            Transactions = transactions,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    private void ValidateTransaction(int pageNumber, int pageSize, DateTime? fromDate, DateTime? toDate)
    {
        if (pageNumber < 1 || pageSize < 1)
            throw new ArgumentException("Page number and page size must be greater than zero.");

        if (fromDate != null)
        {
            toDate ??= DateTime.UtcNow;
            if (fromDate > toDate) throw new ArgumentException("The fromDate cannot be greater than toDate.");
        }
    }

    [HttpPut("expense-tag")]
    public async Task<IActionResult> UpdateExpenseTags([FromBody] UpdateExpenseTagRequest request)
    {
        var transaction = _dbContext.Transactions.Find(request.TransactionId);
        if (transaction == null) return NotFound();

        if (request.ExpenseTagId != null)
        {
            var expenseTag = _dbContext.ExpenseTag.Find(request.ExpenseTagId);
            transaction.ExpenseTag = expenseTag;
            transaction.ExpenseTagId = expenseTag?.Id;
        }
        else
        {
            transaction.ExpenseTag = null;
            transaction.ExpenseTagId = null;
        }

        _dbContext.Entry(transaction).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();

        return Ok("Transaction updated");
    }
}