using BankoApi.Data.Dao;

namespace BankoApi.Controllers.Settings.Responses;

public class GetExpenseTagsResponse
{
    public List<ExpenseTag> ExpenseTags { get; set; }
}
