using BankoApi.Controllers.Shared;
using BankoApi.Controllers.GoCardless.Responses;
using BankoApi.Data;
using BankoApi.Exceptions.GoCardless.Transactions;
using BankoApi.Repository;
using BankoApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Controllers.GoCardless;

[ApiController]
[Authorize]
[Route("gocardless/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly BankoDbContext _dbContext;
    private readonly GoCardlessService _goCardlessService;
    private readonly TransactionsRepository _repository;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(GoCardlessService goCardlessService, BankoDbContext dbContext, TransactionsRepository repository, ILogger<TransactionsController> logger)
    {
        _goCardlessService = goCardlessService;
        _dbContext = dbContext;
        _repository = repository;
        _logger = logger;
    }

    [HttpGet("{bankAccountId}")]
    public async Task<IActionResult> FetchAndStoreTransactions(Guid bankAccountId)
    {
        try
        {
            var userId = User.GetUserId();

            var ownsAccount = await _dbContext.BankAccounts
                .AnyAsync(ba => ba.BankAccountId == bankAccountId.ToString() && ba.BankAuthorization.UserId == userId);
            if (!ownsAccount) return Forbid();

            var transactions = await _goCardlessService.GetTransactionsAsync(bankAccountId);

            if (transactions == null) return NotFound(FetchAndStoreTransactionResponse.NoTransactionsFound.ToString());
            await _repository.StoreTransactions(ctx: _dbContext, userId: userId, bankAccountId: bankAccountId, transactions);

            await _dbContext.SaveChangesAsync();
            return Ok(FetchAndStoreTransactionResponse.TransactionsStoredSuccessfully.ToString());
        }
        catch (EndUserAgreementException ex)
        {
            _logger.LogError(ex, "End user agreement expired");
            _repository.SetEuaExpirationStatus(_dbContext, ex.Message);
            return Unauthorized(new ErrorResponse()
            {
                Message = FetchAndStoreTransactionResponse.EndUserAgreementExpired.ToString()
            });
        }
    }
}