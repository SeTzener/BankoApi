using BankoApi.Controllers.Shared.DTO;
using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Exceptions.User;

namespace BankoApi.Repository;

public class UserRepository
{
    private const int WorkFactor = 12; // OWASP recommends 10-12

    public Guid CreateAccount(BankoDbContext context, UserDto userData)
    {
        User? user = context.Users.FirstOrDefault(user => user.Email == userData.Email);

        if (user != null)
        {
            if (user.IsActive) throw new EmailConflictException($"The email {userData.Email} already exists.");

            user.IsActive = true;
            user.PasswordHash = HashPassword(userData.Password);
            user.FullName = userData.FullName;
            user.Address = userData.Address;
            user.PhoneNumber = userData.PhoneNumber;
            user.ConsentGiven = userData.ConsentGiven ?? false;

            return user.UserId;
        }

        var id = Guid.NewGuid();
        var newUser = new User
        {
            UserId = id,
            Email = userData.Email,
            PasswordHash = HashPassword(userData.Password),
            FullName = userData.FullName,
            Address = userData.Address,
            PhoneNumber = userData.PhoneNumber,
            ConsentGiven = userData.ConsentGiven ?? false
        };
        context.Users.Add(newUser);

        return id;
    }

    public Guid ValidateAccount(BankoDbContext context, string email, string password)
    {
        var user = context.Users.FirstOrDefault(user => user.Email == email);
        if (user == null) throw new EmailNotFoundException($"The email {email} was not found in the DB");

        if (!user.IsActive) throw new InactiveUserException($"The user {user.UserId} is not active");

        var isPasswordValid = VerifyPassword(password, user.PasswordHash);
        if (!isPasswordValid)
            throw new PasswordNotFoundException($"The password for the UserId {user.UserId} is not valid");

        user.LastLoginAt = DateTime.Now;
        context.SaveChanges();
        
        return user!.UserId;
    }

    public void UpdatePassword(BankoDbContext context, Guid userId, string currentPassword, string newPassword)
    {
        var user = context.Users.Find(userId)
            ?? throw new EmailNotFoundException($"User {userId} not found");

        if (!VerifyPassword(currentPassword, user.PasswordHash))
            throw new PasswordNotFoundException("Current password is incorrect");

        user.PasswordHash = HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        context.SaveChanges();
    }

    public void SoftDeleteUser(BankoDbContext context, Guid userId)
    {
        var user = context.Users.Find(userId)
            ?? throw new EmailNotFoundException($"User {userId} not found");

        user.Email = $"deleted-{userId:N}@anon.banko";
        user.FullName = null;
        user.PhoneNumber = null;
        user.Address = null;
        user.PasswordHash = null;
        user.IsActive = false;
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        user.ConsentGiven = false;
        user.ConsentUpdatedAt = null;
        user.ConsentVersionId = null;

        var activeTokens = context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked);
        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
        }

        context.SaveChanges();
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