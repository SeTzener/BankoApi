using BankoApi.Data.Dao;

namespace BankoApi.Controllers.BankoApi.Controllers.SettingsController.Responses;

public class GetExpenseTagsResponse
{
    public List<ExpenseTag> ExpenseTags { get; set; }
}