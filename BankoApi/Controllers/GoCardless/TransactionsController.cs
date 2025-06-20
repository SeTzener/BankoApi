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
        // TODO(): Handle the exceptions
        var transactions = await _goCardlessService.GetTransactionsAsync(accountId);

        if (transactions == null) return NotFound();
        _repository.StoreTransactions(_dbContext, transactions);

        await _dbContext.SaveChangesAsync();
        return Ok("Transactions stored successfully.");
    }
}