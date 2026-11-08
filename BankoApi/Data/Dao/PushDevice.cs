using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankoApi.Data.Dao;

[Table("PushDevices")]
public class PushDevice
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    // FCM registration token for this app installation.
    [Required]
    [MaxLength(512)]
    public string Token { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public PushPlatform Platform { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastSeenAt { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; }
}

public enum PushPlatform
{
    Android,
    Ios
}
