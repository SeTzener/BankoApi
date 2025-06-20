namespace BankoApi.Data.Dao;

public class ExpenseTag
{
    public string Id { get; set; }
    public required string Name { get; set; }
    public required long Color { get; set; }
    public required bool isEarning { get; set; } = false;
    public List<string>? Aka { get; set; }
}