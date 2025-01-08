namespace BankoApi.Data.Dao;

using System.Collections.Generic;

public class Institution
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string BIC { get; set; }
    public int TransactionTotalDays { get; set; }
    public List<string> Countries { get; set; }
    public string Logo { get; set; }
    public int MaxAccessValidForDays { get; set; }
}
