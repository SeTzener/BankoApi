using System.ComponentModel.DataAnnotations;

namespace BankoApi.Controllers.Users.Requests;

public class RefreshTokenRequest
{
    [Required] public string RefreshToken { get; set; } = string.Empty;
}
