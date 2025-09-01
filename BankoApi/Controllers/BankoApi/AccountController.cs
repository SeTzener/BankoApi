using BankoApi.Controllers.BankoApi.DTO;
using BankoApi.Controllers.BankoApi.Requests;
using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Repository;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.BankoApi
{
    [ApiController]
    [Route("[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly BankoDbContext _dbContext;
        private AccountRepository _repository;

        public AccountsController(BankoDbContext dbContext)
        {
            _dbContext = dbContext;
            _repository = new AccountRepository();
        }

        [HttpPost]
        public async Task<IActionResult> NewAccount([FromBody] NewAccountRequest request)
        {
            Boolean isCreated = _repository.CreateAccount(
                accountData: new AccountDto {
                    Email = request.Email,
                    Password = request.Password,
                    FullName = request.FullName,
                    Address = request.Address,
                    PhoneNumber = request.PhoneNumber,
                    ConsentGiven = request.ConsentGiven,
                    },
                context: _dbContext
            );
            await _dbContext.SaveChangesAsync();
            return Ok("Account created");
        }
    }
}