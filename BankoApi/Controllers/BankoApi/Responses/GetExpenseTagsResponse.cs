using BankoApi.Data.Dao;

namespace BankoApi.Controllers.BankoApi.Responses;

public class GetExpenseTagsResponse
{
    public List<ExpenseTag> ExpenseTags { get; set; }
}