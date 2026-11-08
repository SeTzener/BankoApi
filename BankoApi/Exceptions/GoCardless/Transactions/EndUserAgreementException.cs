namespace BankoApi.Exceptions.GoCardless.Transactions
{
    public class EndUserAgreementException : Exception
    {
        public EndUserAgreementException(string message) : base(message) { }
    }
}
