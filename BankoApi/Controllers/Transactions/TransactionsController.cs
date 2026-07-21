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

        var userId = User.GetUserId();

        toDate ??= DateTime.UtcNow;
        fromDate ??= await _dbContext.Transactions
            .Where(t => t.UserId == userId)
            .DefaultIfEmpty()
            .MinAsync(t => (DateTime?)t.BookingDate) ?? DateTime.UtcNow.AddYears(-1);

        var skip = (pageNumber - 1) * pageSize;

        // Query filtered by userId and date range
        var query = _dbContext.Transactions.Where(t => t.UserId == userId && t.BookingDate >= fromDate && t.BookingDate <= toDate);

        var totalCount = query.Count();

        // First get just the IDs with pagination
        var transactionIds = await _dbContext.Transactions
            .Where(t => t.UserId == userId &&
                        t.isDeleted != true &&
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
            .AsNoTracking()
            .Include(t => t.DebtorAccount)
            .Include(t => t.CreditorAccount)
            .Include(t => t.ExpenseTag)
            .OrderByDescending(t => t.BookingDate)
            .ToListAsync();

        // Manual join: look up BankAccount by matching BankAccountId (GC UUID string)
        // instead of relying on a DB FK that maps Transaction.BankAccountId to BankAccounts.Id
        var gcAccountIds = transactions
            .Select(t => t.BankAccountId.ToString())
            .Distinct()
            .ToList();
        var bankAccounts = await _dbContext.BankAccounts
            .Where(ba => gcAccountIds.Contains(ba.BankAccountId))
            .AsNoTracking()
            .Include(ba => ba.BankAuthorization)
                .ThenInclude(ba => ba.Institution)
            .ToListAsync();
        var bankAccountLookup = bankAccounts.ToDictionary(ba => Guid.Parse(ba.BankAccountId));

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
            BankName = bankAccountLookup.GetValueOrDefault(t.BankAccountId)?.BankAuthorization?.Institution?.Name,
            BankLogoUrl = bankAccountLookup.GetValueOrDefault(t.BankAccountId)?.BankAuthorization?.Institution?.LogoUrl
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
        var userId = User.GetUserId();
        var transaction = _dbContext.Transactions.Find(transactionId);
        if (transaction == null) return NotFound();
        if (transaction.UserId != userId) return Forbid();
        transaction.isDeleted = true;
        await _dbContext.SaveChangesAsync();
        return Ok("Transaction updated");
    }

    [HttpPut("expense-tag")]
    public async Task<IActionResult> UpdateExpenseTags([FromBody] UpdateExpenseTagRequest request)
    {
        var userId = User.GetUserId();
        var transaction = _dbContext.Transactions.Find(request.TransactionId);
        if (transaction == null) return NotFound();
        if (transaction.UserId != userId) return Forbid();

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
        var userId = User.GetUserId();
        var transaction = _dbContext.Transactions.Find(transactionId);
        if (transaction == null) return NotFound();
        if (transaction.UserId != userId) return Forbid();
        transaction.Note = request.Note;
        _dbContext.Entry(transaction).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
        return Ok("Transaction updated");
    }
}
