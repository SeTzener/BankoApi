namespace BankoApi.Data.Dao;

public class ExpenseTag
{
    public String Id { get; set; }
    public required String Name { get; set; }
    public required Int64 Color { get; set; }
    public required Boolean isEarning { get; set; } = false;
    public List<String>? Aka { get; set; }
}