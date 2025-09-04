using BankoApi.Controllers.BankoApi.DTO;
using BankoApi.Controllers.BankoApi.Responses;
using BankoApi.Data;
using BankoApi.Data.Dao;

namespace BankoApi.Repository;

public class AccountRepository
{
    private const int WorkFactor = 12; // OWASP recommends 10-12
    public Guid CreateAccount(BankoDbContext context, AccountDto accountData)
    {
        Boolean isExistingAccount = context.Accounts.Any(account => account.Email == accountData.Email);
        if (isExistingAccount)
        {
            
        }
        Guid id = Guid.NewGuid();
        Account account = new Account()
        {
            AccountId = id,
            Email = accountData.Email,
            PasswordHash = HashPassword(accountData.Password),
            FullName = accountData.FullName,
            Address = accountData.Address,
            PhoneNumber = accountData.PhoneNumber,
            ConsentGiven = accountData.ConsentGiven ?? false,
        };
        context.Accounts.Add(account);
        return id;
    }

    public Guid ValidateAccount(BankoDbContext context, String email, String password)
    {
        Account? account = context.Accounts.FirstOrDefault(account => account.Email == email);
        if (account == null)
        {
            throw new Exception(AccountErrorMessages.WrongCredentials.ToString());
        }
        
        Boolean isPasswordValid = VerifyPassword(password: password, hashedPassword: account.PasswordHash);
        if (!isPasswordValid)
        {
            throw new Exception(AccountErrorMessages.WrongCredentials.ToString());
        }
        
        return account!.AccountId;
    }
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, WorkFactor);
    }
    
    private bool VerifyPassword(string password, string? hashedPassword)
    {
        if (hashedPassword == null)
        {
            return false;
        }
        return BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
    }
}