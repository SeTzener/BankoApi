namespace BankoApi.Exceptions.Settings
{
    public class NoBankAuthorizationFoundException : Exception
    {
        public NoBankAuthorizationFoundException(string message) : base(message) { }
    }
}
