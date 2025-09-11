namespace BankoApi.Data.Dao;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class BankAccount
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int ConnectionId { get; set; }

    [Required]
    [MaxLength(255)]
    public string AccountId { get; set; } // The only non-nullable field from GC

    // All details are fetched in a separate step and can be empty/null
    [MaxLength(100)]
    public string? Iban { get; set; } // Nullable

    [MaxLength(100)]
    public string? Bban { get; set; } // Nullable

    [MaxLength(10)]
    public string? Currency { get; set; } // Nullable

    [MaxLength(255)]
    public string? OwnerName { get; set; } // Nullable

    [MaxLength(100)]
    public string? Product { get; set; } // Nullable

    [MaxLength(255)]
    public string? AccountName { get; set; } // Nullable

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    [ForeignKey("ConnectionId")]
    public virtual BankAuthorization BankAuthorization { get; set; }
}