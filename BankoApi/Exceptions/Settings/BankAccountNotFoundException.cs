namespace BankoApi.Exceptions.Settings
{
    public class BankAccountNotFoundException : Exception
    {
        public BankAccountNotFoundException(string message) : base(message)
        {
        }
    }
}
