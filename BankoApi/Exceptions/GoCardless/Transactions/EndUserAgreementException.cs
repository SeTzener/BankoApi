namespace BankoApi.Exceptions.GoCardless.Transactions
{
    public class EndUserAgreementException : Exception
    {
        public EndUserAgreementException(string message) : base(message) { }
    }

    public static class EndUserAgreementExceptionMessages
    {
        public const string Message = "EUA was valid for 90 days and it expired";
    }
}
