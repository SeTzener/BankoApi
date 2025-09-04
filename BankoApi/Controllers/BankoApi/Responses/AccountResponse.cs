using System.ComponentModel;

namespace BankoApi.Controllers.BankoApi.Responses;

public class AccountResponse
{
    public Guid AccountId { get; set; }
    public String AccessToken { get; set; }
    public String RefreshToken { get; set; }
    public Int64 ExpiresIn { get; set; }
}


enum AccountErrorMessages
{
    WrongCredentials,
    
}