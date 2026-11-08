namespace BankoApi.Data.Dao;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class BankAuthorization
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [MaxLength(50)]
    public String? RequisitionId { get; set; }

    [MaxLength(50)]
    public String? InstitutionId { get; set; }

    [Required] [MaxLength(50)] 
    public BankAuthorizationStaus Status { get; set; } = BankAuthorizationStaus.Processing;

    [MaxLength(50)]
    public String? AgreementId { get; set; }

    [MaxLength(50)]
    public String? ReferenceId { get; set; }

    // Metadata for UX
    [MaxLength(255)]
    public String? InstitutionName { get; set; } // Nullable

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // EUA lifecycle. AgreementExpiresAt is the source of truth for renewal warnings.
    public DateTime? AgreementCreatedAt { get; set; }
    public DateTime? AgreementAcceptedAt { get; set; }
    public DateTime? AgreementExpiresAt { get; set; }
    public int? AccessValidForDays { get; set; }
    public DateTime? LastStatusCheckAt { get; set; }

    // Navigation Properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
    [ForeignKey("BankAccountId")]
    public virtual ICollection<BankAccount>? BankAccounts { get; set; }
    public virtual Institution? Institution { get; set; }
}

public enum BankAuthorizationStaus
{ 
    Processing,
    Linked,
    Error,
    Expired
}