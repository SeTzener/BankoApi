namespace BankoApi.Exceptions.User;

public class EmailConflictException : Exception
{
    public EmailConflictException(string message) : base(message)
    {
    }
}