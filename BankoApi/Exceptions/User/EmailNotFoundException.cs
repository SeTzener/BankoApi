namespace BankoApi.Exceptions.User;

public class EmailNotFoundException : Exception
{
    public EmailNotFoundException(string message) : base(message)
    {
    }
}