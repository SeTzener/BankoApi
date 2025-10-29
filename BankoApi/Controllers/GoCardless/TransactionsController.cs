using BankoApi.Controllers.BankoApi.Controllers.Responses;
using BankoApi.Controllers.GoCardless.Responses;
using BankoApi.Data;
using BankoApi.Exceptions.GoCardless.Transactions;
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

    [HttpGet("{bankAccountId}")]
    public async Task<IActionResult> FetchAndStoreTransactions(Guid bankAccountId)
    {
        try
        {
            var transactions = await _goCardlessService.GetTransactionsAsync(bankAccountId);
            Guid userId = _dbContext.Users.FirstOrDefault()?.UserId ?? throw new ArgumentNullException(nameof(bankAccountId));

            if (transactions == null) return NotFound(FetchAndStoreTransactionResponse.NoTransactionsFound.ToString());
            _repository.StoreTransactions(ctx: _dbContext, userId: userId, bankAccountId: bankAccountId, transactions);

            await _dbContext.SaveChangesAsync();
            return Ok(FetchAndStoreTransactionResponse.TransactionsStoredSuccessfully.ToString());
        }
        catch (EndUserAgreementException ex)
        {
            Console.WriteLine(ex.Message);
            _repository.SetEuaExpirationStatus(_dbContext, ex.Message);
            return Unauthorized(new ErrorResponse()
            {
                Message = FetchAndStoreTransactionResponse.EndUserAgreementExpired.ToString()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest(new ErrorResponse() { Message = FetchAndStoreTransactionResponse.SomethingWentWrong.ToString() });
        }
    }
}