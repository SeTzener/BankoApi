using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankoApi.Data.Dao;

[Table("PrivacyPolicyVersions")]
public class PrivacyPolicyVersion
{
    [Key] public Guid Id { get; set; }

    [Required] public int Version { get; set; }

    [Required] [StringLength(500)] public string Title { get; set; } = string.Empty;

    [Required] public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    [Required] public bool IsActive { get; set; } = true;
}
