using BankoApi.Controllers.BankoApi.DTO;
using BankoApi.Data;
using BankoApi.Data.Dao;

namespace BankoApi.Repository;

public class AccountRepository
{
    public Boolean CreateAccount(AccountDto accountData, BankoDbContext context)
    {
        // Fare l'hash della password prima di salvarla
        Account account = new Account()
        {
            AccountId = Guid.NewGuid(),
            Email = accountData.Email,
            PasswordHash = accountData.Password,
            FullName = accountData.FullName,
            Address = accountData.Address,
            PhoneNumber = accountData.PhoneNumber,
            ConsentGiven = accountData.ConsentGiven ?? false,
        };
        context.Accounts.Add(account);
        return true;
    }
}