using BankoApi.Data;
using BankoApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.Nordigen;

[ApiController]
[Route("[controller]")]
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

        // Store in database
        _dbContext.Transactions.Add(transactions);


        await _dbContext.SaveChangesAsync();
        return Ok("Transactions stored successfully.");
    }
}
