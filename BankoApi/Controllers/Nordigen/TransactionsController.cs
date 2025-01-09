using BankoApi.Data;
using BankoApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BankoApi.Controllers.Nordigen;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly NordigenService _nordigenService;
    private readonly BankoDbContext _dbContext;

    public TransactionsController(NordigenService nordigenService, BankoDbContext dbContext)
    {
        _nordigenService = nordigenService;
        _dbContext = dbContext;
    }

    [HttpGet("{accountId}")]
    public async Task<IActionResult> FetchAndStoreTransactions(string accountId)
    {
        // Fetch transactions from Nordigen
        var transactions = await _nordigenService.GetTransactionsAsync(accountId);

        if (transactions.IsNullOrEmpty() || !transactions.Any())
        {
            return NotFound("No transactions found.");
        }

        // Store in database
        foreach (var transaction in transactions)
        {
            if (!_dbContext.Transactions.Any(t => t.Id == transaction.Id))
            {
                _dbContext.Transactions.Add(transaction);
            }
        }

        await _dbContext.SaveChangesAsync();
        return Ok("Transactions stored successfully.");
    }
}
