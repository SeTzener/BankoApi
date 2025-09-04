namespace BankoApi.Exceptions.Account;

public class EmailNotFoundException : Exception
{
    public EmailNotFoundException(string message) : base(message)
    {
    }
}