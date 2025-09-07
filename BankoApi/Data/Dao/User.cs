using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankoApi.Data.Dao;

[Table("Users")]
public class User
{
    [Key] public Guid UserId { get; set; }

    [Required] [StringLength(255)] public string Email { get; set; } = string.Empty;

    [StringLength(255)] public string? PasswordHash { get; set; } = string.Empty;

    [StringLength(255)] public string? FullName { get; set; }

    [StringLength(50)] public string? PhoneNumber { get; set; }

    [StringLength(500)] public string? Address { get; set; }

    [Required] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeletedAt { get; set; }

    [Required] public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    [Required] public bool ConsentGiven { get; set; } = false;

    public DateTime? ConsentUpdatedAt { get; set; }
}