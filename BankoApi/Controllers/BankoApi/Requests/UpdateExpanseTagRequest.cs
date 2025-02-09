namespace BankoApi.Controllers.BankoApi.Requests;

public class UpdateExpenseTagRequest
{
    public string ExpenseTagId { get; set; }
    public string TransactionId { get; set; }
}