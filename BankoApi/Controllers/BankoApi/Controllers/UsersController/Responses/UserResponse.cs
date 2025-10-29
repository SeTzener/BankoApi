namespace BankoApi.Controllers.BankoApi.Controllers.UsersController.Responses;

public class UserResponse
{
    public Guid AccountId { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public long ExpiresIn { get; set; }
}

internal enum UserErrorMessages
{
    WrongCredentials,
    EmailAlreadyExists,
    SomethingWentWrong,
    InactiveAccount
}