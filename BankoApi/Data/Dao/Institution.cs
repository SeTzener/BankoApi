using System.ComponentModel.DataAnnotations;

namespace BankoApi.Data.Dao;

public class Institution
{
    [Key]
    [MaxLength(50)]
    public required string Id { get; set; }

    [MaxLength(255)]
    public required string Name { get; set; }

    public string? LogoUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
