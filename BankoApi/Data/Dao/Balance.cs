namespace BankoApi.Data.Dao;

public class Balance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BalanceType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}