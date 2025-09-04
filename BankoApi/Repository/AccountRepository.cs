using BankoApi.Controllers.BankoApi.DTO;
using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Exceptions.Account;

namespace BankoApi.Repository;

public class AccountRepository
{
    private const int WorkFactor = 12; // OWASP recommends 10-12

    public Guid CreateAccount(BankoDbContext context, AccountDto accountData)
    {
        Account? account = context.Accounts.FirstOrDefault(account => account.Email == accountData.Email);

        if (account != null)
        {
            if (account.IsActive) throw new EmailConflictException($"The email {accountData.Email} already exists.");

            account.IsActive = true;
            account.PasswordHash = HashPassword(accountData.Password);
            account.FullName = accountData.FullName;
            account.Address = accountData.Address;
            account.PhoneNumber = accountData.PhoneNumber;
            account.ConsentGiven = accountData.ConsentGiven ?? false;

            return account.AccountId;
        }

        var id = Guid.NewGuid();
        var newAccount = new Account
        {
            AccountId = id,
            Email = accountData.Email,
            PasswordHash = HashPassword(accountData.Password),
            FullName = accountData.FullName,
            Address = accountData.Address,
            PhoneNumber = accountData.PhoneNumber,
            ConsentGiven = accountData.ConsentGiven ?? false
        };
        context.Accounts.Add(newAccount);

        return id;
    }

    public Guid ValidateAccount(BankoDbContext context, string email, string password)
    {
        var account = context.Accounts.FirstOrDefault(account => account.Email == email);
        if (account == null) throw new EmailNotFoundException($"The email {email} was not found in the DB");

        if (!account.IsActive) throw new InactiveAccountException($"The account {account.AccountId} is not active");

        var isPasswordValid = VerifyPassword(password, account.PasswordHash);
        if (!isPasswordValid)
            throw new PasswordNotFoundException($"The password for the AccountId {account.AccountId} is not valid");

        account.LastLoginAt = DateTime.Now;
        context.SaveChanges();
        
        return account!.AccountId;
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, WorkFactor);
    }

    private bool VerifyPassword(string password, string? hashedPassword)
    {
        if (hashedPassword == null) return false;
        return BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
    }
}