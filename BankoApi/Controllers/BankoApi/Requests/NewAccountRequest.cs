using System.ComponentModel.DataAnnotations;

namespace BankoApi.Controllers.BankoApi.Requests;

public class NewAccountRequest
{
    [EmailAddress] public string Email { get; set; }

    [Required] [MinLength(10)] public string Password { get; set; }

    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public bool ConsentGiven { get; set; }
}