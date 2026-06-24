namespace BankoApi.Controllers.Users.Responses;

public class UserProfileResponse
{
    public Guid AccountId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public bool ConsentGiven { get; set; }
    public DateTime? ConsentUpdatedAt { get; set; }
    public Guid? ConsentVersionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
