namespace BankoApi.Exceptions.User;

public class PasswordNotFoundException : Exception
{
    public PasswordNotFoundException(string message) : base(message)
    {
    }
}