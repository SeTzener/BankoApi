using System.ComponentModel.DataAnnotations;

namespace BankoApi.Controllers.Users.Requests;

public class ChangePasswordRequest
{
    [Required] public string CurrentPassword { get; set; } = string.Empty;

    [Required] [MinLength(10)] public string NewPassword { get; set; } = string.Empty;
}
