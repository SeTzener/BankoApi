using BankoApi.Controllers.BankoApi.Controllers.Responses;
using BankoApi.Controllers.BankoApi.Controllers.SettingsController.Requests;
using BankoApi.Controllers.BankoApi.Controllers.SettingsController.Responses;
using BankoApi.Data.Dao;
using BankoApi.Exceptions.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BankoApi.Controllers.BankoApi.Controllers.SettingsController.SettingsController
{
    public partial class SettingsController
    {
        [HttpGet("BankAccount")]
        public async Task<IActionResult> GetBankAccount([FromQuery] List<String> bankAccountId)
        {
            try
            {
                if (bankAccountId.IsNullOrEmpty()) throw new ArgumentNullException(nameof(bankAccountId));

                List<BankAccountResponse> bankAccounts = new List<BankAccountResponse>();

                foreach (var id in bankAccountId)
                {
                    var result = await _dbContext.BankAccounts.FirstOrDefaultAsync(ba => ba.BankAccountId == id)
                            ?? throw new BankAccountNotFoundException(
                                message: $"The requested BankAccountId: {bankAccountId} was not found.");
                    bankAccounts.Add(new BankAccountResponse() {
                        BankAuthorizationId = result.BankAuthorizationId,
                        BankAccountId = result.BankAccountId,
                        Iban = result.Iban,
                        Bban = result.Bban,
                        Currency = result.Currency,
                        OwnerName = result.OwnerName,
                        Product = result.Product,
                        AccountName = result.AccountName,
                        CreatedAt = result.CreatedAt,
                        UpdatedAt = result.UpdatedAt,
                    });
                }

                return Ok(new GetBankAccountsResponse(){ 
                    Accounts = bankAccounts 
                });
            }
            catch (BankAccountNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound(new ErrorResponse() { Message = GetBankAccountErrorMessages.AccountNotFound.ToString() });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new ErrorResponse() { Message = ex.Message });
            }
        }

        [HttpPut("BankAccount")]
        public async Task<IActionResult> UpsertBankAccount([FromBody] UpsertBankAccountRequest request)
        {
            try
            {
                var account = await _dbContext.BankAccounts.FirstOrDefaultAsync(a => a.BankAccountId == request.AccountId)
                    ?? new BankAccount()
                    {
                        BankAccountId = request.AccountId,
                        BankAuthorizationId = request.BankAuthorizationId
                    };

                if (request.Iban != null)
                    account.Iban = request.Iban;

                if (request.Bban != null)
                    account.Bban = request.Bban;

                if (request.OwnerName != null)
                    account.OwnerName = request.OwnerName;

                if (request.Currency != null)
                    account.Currency = request.Currency;

                if (request.Product != null)
                    account.Product = request.Product;

                if (request.AccountName != null)
                    account.AccountName = request.AccountName;

                if (account.Id == Guid.Empty)
                {
                    _dbContext.Add(account);
                }
                else
                {
                    account.UpdatedAt = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new UpsertBankAccountResponse()
                {
                    BankAuthorizationId = account.BankAuthorizationId,
                    AccountId = account.BankAccountId,
                    Iban = account.Iban,
                    Bban = account.Bban,
                    OwnerName = account.OwnerName,
                    Currency = account.Currency,
                    Product = account.Product,
                    AccountName = account.AccountName
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new ErrorResponse() { Message = ex.Message });
            }
        }
    }
}
