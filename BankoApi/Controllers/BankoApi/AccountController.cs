using BankoApi.Controllers.BankoApi.DTO;
using BankoApi.Controllers.BankoApi.Requests;
using BankoApi.Controllers.BankoApi.Responses;
using BankoApi.Controllers.BankoApi.Utils;
using BankoApi.Data;
using BankoApi.Exceptions.Account;
using BankoApi.Repository;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.BankoApi;

[ApiController]
[Route("[controller]")]
public class AccountsController : ControllerBase
{
    private readonly BankoDbContext _dbContext;
    private readonly AccountRepository _repository;

    public AccountsController(BankoDbContext dbContext)
    {
        _dbContext = dbContext;
        _repository = new AccountRepository();
    }

    [HttpPost]
    public async Task<IActionResult> NewAccount([FromBody] NewAccountRequest request)
    {
        try
        {
            var accountId = _repository.CreateAccount(
                accountData: new AccountDto
                {
                    Email = request.Email,
                    Password = request.Password,
                    FullName = request.FullName,
                    Address = request.Address,
                    PhoneNumber = request.PhoneNumber,
                    ConsentGiven = request.ConsentGiven
                },
                context: _dbContext
            );
            await _dbContext.SaveChangesAsync();
            return Ok(new AccountResponse
            {
                AccountId = accountId,
                AccessToken = "ACCESS_TOKEN",
                RefreshToken = "REFRESH_TOKEN",
                ExpiresIn = 1234567890
            });
        }
        catch (EmailConflictException ex)
        {
            Console.WriteLine(ex.Message);
            return Conflict(new ErrorResponse
            {
                Message = AccountErrorMessages.EmailAlreadyExists.ToString()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest(new ErrorResponse
            {
                Message = AccountErrorMessages.SomethingWentWrong.ToString()
            });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var accountId = _repository.ValidateAccount(_dbContext, request.Email, request.Password);

            return Ok(new AccountResponse
            {
                AccountId = accountId,
                AccessToken = "ACCESS_TOKEN",
                RefreshToken = "REFRESH_TOKEN",
                ExpiresIn = 1234567890
            });
        }
        catch (Exception ex) when (ex is EmailNotFoundException || ex is PasswordNotFoundException)
        {
            Console.WriteLine(ex.Message);
            return Unauthorized(new ErrorResponse
            {
                Message = AccountErrorMessages.WrongCredentials.ToString()
            });
        }
        catch (InactiveAccountException ex)
        {
            Console.WriteLine(ex.Message);
            return this.Forbidden(new ErrorResponse
            {
                Message = AccountErrorMessages.InactiveAccount.ToString()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest(new ErrorResponse
            {
                Message = AccountErrorMessages.SomethingWentWrong.ToString()
            });
        }
    }
}