using BankoApi.Data;
using BankoApi.Repository;
using BankoApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.GoCardless;

[ApiController]
[Route("gocardless/[controller]")]
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

    [HttpGet("{accountId}")]
    public async Task<IActionResult> FetchAndStoreTransactions(string accountId)
    {
        // TODO(): This Endpoint has to change completely. The accountId in the parameter has to be the BankoAccountId
        // Then it has to loop through the GoCardless table and retrieve the GoCardless account IDs. 
        var transactions = await _goCardlessService.GetTransactionsAsync(accountId);
        Guid bankoAccountId = _dbContext.Accounts.FirstOrDefault()?.AccountId ?? throw new ArgumentNullException(nameof(accountId));

        if (transactions == null) return NotFound();
        _repository.StoreTransactions(ctx:_dbContext, accountId: bankoAccountId, transactions);

        await _dbContext.SaveChangesAsync();
        return Ok("Transactions stored successfully.");
    }
}