using System.ComponentModel.DataAnnotations;

namespace BankoApi.Controllers.BankoApi.Requests;

public class NewAccountRequest
{
    [EmailAddress]
    public String Email { get; set; }
    [Required]
    [MinLength(10)]
    public String Password { get; set; }
    public String? FullName { get; set; }
    public String? Address { get; set; }
    public String? PhoneNumber { get; set; }
    public Boolean ConsentGiven { get; set; }
}