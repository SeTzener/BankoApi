namespace BankoApi.Controllers.BankoApi.Controllers.SettingsController.Requests;

public class UpdateExpenseTagRequest
{
    public string? ExpenseTagId { get; set; }
    public string TransactionId { get; set; }
}