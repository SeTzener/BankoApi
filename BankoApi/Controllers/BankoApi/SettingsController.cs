using BankoApi.Controllers.BankoApi.Responses;
using BankoApi.Data;
using BankoApi.Data.Dao;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.BankoApi;

[ApiController]
[Route("[controller]")]
public class SettingsController : ControllerBase
{
    private readonly BankoDbContext _dbContext;

    public SettingsController(BankoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("expense-tags")]
    public async Task<IActionResult> GetAllTagsAsync()
    {
        return Ok(new GetExpenseTagsResponse
        {
            ExpenseTags = _dbContext.ExpenseTag.ToList()
        });
    }

    [HttpPost("expense-tag")]
    public async Task<IActionResult> AddExpenseTagAsync([FromBody] ExpenseTag expenseTag)
    {
        _dbContext.ExpenseTag.Add(expenseTag);
        await _dbContext.SaveChangesAsync();
        return Ok(new UpsertExpenseTagResponse { ExpenseTag = expenseTag });
    }

    [HttpPut("expense-tag/{expenseTagId}")]
    public async Task<IActionResult> UpdateExpenseTagAsync(string expenseTagId, [FromBody] ExpenseTag expenseTag)
    {
        var tag = _dbContext.ExpenseTag.Find(expenseTagId);
        if (tag == null) return NotFound();

        tag.Name = expenseTag.Name;
        tag.Color = expenseTag.Color;
        tag.isEarning = expenseTag.isEarning;
        tag.Aka = expenseTag.Aka;
        _dbContext.ExpenseTag.Update(tag);
        await _dbContext.SaveChangesAsync();

        return Ok(new UpsertExpenseTagResponse { ExpenseTag = expenseTag });
    }

    [HttpDelete("expense-tag/{expenseTagId}")]
    public async Task<IActionResult> DeleteExpenseTagAsync(string expenseTagId)
    {
        var tag = _dbContext.ExpenseTag.Find(expenseTagId);
        if (tag == null) return NotFound();
        _dbContext.ExpenseTag.Remove(tag);
        await _dbContext.SaveChangesAsync();

        return Ok("Tag deleted");
    }
}