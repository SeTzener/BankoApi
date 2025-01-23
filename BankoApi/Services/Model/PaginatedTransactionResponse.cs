using BankoApi.Data.Dao;

namespace BankoApi.Services.Model;

public class PaginatedTransactionResponse
{
    public required List<Transaction> Transactions { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}