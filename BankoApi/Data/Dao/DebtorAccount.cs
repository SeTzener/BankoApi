namespace BankoApi.Data.Dao;

public class DebtorAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Iban { get; set; }
    public required string Bban { get; set; }
}