namespace BankoApi.Exceptions.User;

public class InactiveUserException : Exception
{
    public InactiveUserException(string message) : base(message)
    {
    }
}