using System.Runtime.InteropServices.JavaScript;

namespace BankoApi.Controllers.BankoApi.DTO;

public struct AccountDto
{
    public String Email { get; init; }
    public String Password { get; init; }
    public String? FullName { get; init; }
    public String? Address { get; init; }
    public String? PhoneNumber { get; init; }
    public Boolean? ConsentGiven { get; init; }
}