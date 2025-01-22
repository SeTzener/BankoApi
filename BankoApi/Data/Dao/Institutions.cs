namespace BankoApi.Data.Dao;

public class Institutions
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string BIC { get; set; }
    public int TransactionTotalDays { get; set; }
    public List<string> Countries { get; set; }
    public string Logo { get; set; }
    public int MaxAccessValidForDays { get; set; }
}