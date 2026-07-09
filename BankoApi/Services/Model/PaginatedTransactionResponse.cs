namespace BankoApi.Services.Model;

public class PaginatedTransactionResponse
{
    public required List<TransactionResponse> Transactions { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
