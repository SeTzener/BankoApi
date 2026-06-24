using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankoApi.Data.Dao;

[Table("ConsentLogs")]
public class ConsentLog
{
    [Key] public Guid Id { get; set; }

    [Required] public Guid UserId { get; set; }

    [Required] public Guid PrivacyPolicyVersionId { get; set; }

    [Required] public bool Accepted { get; set; }

    [Required] public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("PrivacyPolicyVersionId")]
    public virtual PrivacyPolicyVersion PrivacyPolicyVersion { get; set; } = null!;
}
