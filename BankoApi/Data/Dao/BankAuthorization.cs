namespace BankoApi.Data.Dao;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class BankAuthorization
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(255)]
    public String? RequisitionId { get; set; }

    [Required]
    [MaxLength(100)]
    public String? InstitutionId { get; set; }

    [Required]
    [MaxLength(255)]
    public String? ReferenceId { get; set; }

    [Required] [MaxLength(50)] 
    public BankAuthorizationStaus Status { get; set; } = BankAuthorizationStaus.Processing;
    
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public String? AgreementId { get; set; } // Nullable

    // Metadata for UX
    [MaxLength(255)]
    public String? InstitutionName { get; set; } // Nullable

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
    public virtual ICollection<BankAccount> BankAccounts { get; set; }
}

public enum BankAuthorizationStaus
{ 
    Processing,
    Linked,
    Error,
    Expired
}