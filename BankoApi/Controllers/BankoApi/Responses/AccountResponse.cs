namespace BankoApi.Controllers.BankoApi.Responses;

public class AccountResponse
{
    public Guid AccountId { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public long ExpiresIn { get; set; }
}

internal enum AccountErrorMessages
{
    WrongCredentials,
    EmailAlreadyExists,
    SomethingWentWrong,
    InactiveAccount
}