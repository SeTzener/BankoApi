namespace BankoApi.Exceptions.Account;

public class PasswordNotFoundException : Exception
{
    public PasswordNotFoundException(string message) : base(message)
    {
    }
}