namespace BankoApi.Exceptions.Account;

public class InactiveAccountException : Exception
{
    public InactiveAccountException(string message) : base(message)
    {
    }
}