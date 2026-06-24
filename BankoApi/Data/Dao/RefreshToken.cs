using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankoApi.Data.Dao;

[Table("RefreshTokens")]
public class RefreshToken
{
    [Key] public Guid Id { get; set; }

    [Required] public Guid UserId { get; set; }

    [Required] [StringLength(512)] public string Token { get; set; } = string.Empty;

    [Required] [StringLength(512)] public string JwtId { get; set; } = string.Empty;

    [Required] public bool IsUsed { get; set; }

    [Required] public bool IsRevoked { get; set; }

    [Required] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required] public DateTime ExpiresAt { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
