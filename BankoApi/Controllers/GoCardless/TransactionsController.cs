using BankoApi.Data;
using BankoApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.GoCardless;

[ApiController]
[Route("[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly GoCardlessService _goCardlessService;
    private readonly BankoDbContext _dbContext;

    public TransactionsController(GoCardlessService goCardlessService, BankoDbContext dbContext)
    {
        _goCardlessService = goCardlessService;
        _dbContext = dbContext;
    }

    [HttpGet("{accountId}")]
    public async Task<IActionResult> FetchAndStoreTransactions(string accountId)
    {
        // Fetch transactions from GoCardless
        var transactions = await _goCardlessService.GetTransactionsAsync(accountId);
        
        // Store in database
        _dbContext.Transactions.Add(transactions);


        await _dbContext.SaveChangesAsync();
        return Ok("Transactions stored successfully.");
    }
}
