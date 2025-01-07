namespace BankoApi.Data.Dao;

public class Balance
{
    public int Id { get; set; } // Primary Key for EF
    public string BalanceType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}