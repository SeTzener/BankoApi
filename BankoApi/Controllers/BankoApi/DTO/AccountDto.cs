namespace BankoApi.Controllers.BankoApi.DTO;

public struct AccountDto
{
    public string Email { get; init; }
    public string Password { get; init; }
    public string? FullName { get; init; }
    public string? Address { get; init; }
    public string? PhoneNumber { get; init; }
    public bool? ConsentGiven { get; init; }
}