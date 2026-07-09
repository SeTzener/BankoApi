using BankoApi.Controllers.Settings.Requests;
using BankoApi.Controllers.Transactions.Requests;
using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Services.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Controllers.Transactions;

[ApiController]
[Authorize]
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
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest("Page number and page size must be greater than zero.");

        if (fromDate > toDate)
            return BadRequest("The fromDate cannot be greater than toDate.");

        toDate ??= DateTime.UtcNow;
        fromDate ??= _dbContext.Transactions.Min(t => t.BookingDate);

        var skip = (pageNumber - 1) * pageSize;

        // Query filtered by date range
        var query = _dbContext.Transactions.Where(t => t.BookingDate >= fromDate && t.BookingDate <= toDate);

        var totalCount = query.Count();

        // First get just the IDs with pagination
        var transactionIds = await _dbContext.Transactions
            .Where(t => t.isDeleted != true &&
                        t.BookingDate >= fromDate && 
                        t.BookingDate <= toDate)
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
            .Include(t => t.BankAccount)
                .ThenInclude(ba => ba.BankAuthorization)
                    .ThenInclude(ba => ba.Institution)
            .OrderByDescending(t => t.BookingDate)
            .ToListAsync();

        var response = transactions.ConvertAll(t => new TransactionResponse
        {
            Id = t.Id,
            UserId = t.UserId,
            BankAccountId = t.BankAccountId,
            BookingDate = t.BookingDate,
            ValueDate = t.ValueDate,
            Amount = t.Amount,
            Currency = t.Currency,
            DebtorAccount = t.DebtorAccount,
            RemittanceInformationUnstructured = t.RemittanceInformationUnstructured,
            RemittanceInformationUnstructuredArray = t.RemittanceInformationUnstructuredArray,
            BankTransactionCode = t.BankTransactionCode,
            InternalTransactionId = t.InternalTransactionId,
            CreditorName = t.CreditorName,
            CreditorAccount = t.CreditorAccount,
            DebtorName = t.DebtorName,
            RemittanceInformationStructuredArray = t.RemittanceInformationStructuredArray,
            ExpenseTagId = t.ExpenseTagId,
            Note = t.Note,
            isDeleted = t.isDeleted,
            ExpenseTag = t.ExpenseTag,
            BankName = t.BankAccount?.BankAuthorization?.Institution?.Name,
            BankLogoUrl = t.BankAccount?.BankAuthorization?.Institution?.LogoUrl
        });

        return Ok(new PaginatedTransactionResponse
        {
            Transactions = response,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    [HttpDelete("{transactionId}")]
    public async Task<IActionResult> DeleteTransaction([FromRoute] string transactionId)
    {
        var transaction = _dbContext.Transactions.Find(transactionId);
        if (transaction == null) return NotFound();
        transaction.isDeleted = true;
        await _dbContext.SaveChangesAsync();
        return Ok("Transaction updated");
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

    [HttpPut("{transactionId}/note")]
    public async Task<IActionResult> UpdateNote([FromRoute] string transactionId, [FromBody] UpdateNoteRequest request)
    {
        var transaction = _dbContext.Transactions.Find(transactionId);
        if (transaction == null) return NotFound();
        transaction.Note = request.Note;
        _dbContext.Entry(transaction).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
        return Ok("Transaction updated");
    }
}
