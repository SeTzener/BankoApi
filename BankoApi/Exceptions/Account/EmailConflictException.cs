namespace BankoApi.Exceptions.Account;

public class EmailConflictException : Exception
{
    public EmailConflictException(string message) : base(message)
    {
    }
}