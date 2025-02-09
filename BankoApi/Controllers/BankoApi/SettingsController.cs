using BankoApi.Controllers.BankoApi.Responses;
using BankoApi.Data;
using BankoApi.Data.Dao;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.BankoApi;

[ApiController]
[Route("[controller]")]
public class SettingsController : ControllerBase
{
    private BankoDbContext _dbContext;

    public SettingsController(BankoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("expense-tags")]
    public async Task<IActionResult> GetAllTagsAsync()
    {
        return Ok(new GetExpenseTagsResponse
        {
            ExpenseTags = _dbContext.ExpenseTags.ToList()
        });
    }

    [HttpPost("expense-tag")]
    public async Task<IActionResult> AddExpenseTagAsync([FromBody] ExpenseTag expenseTag)
    {
        _dbContext.ExpenseTags.Add(expenseTag);
        await _dbContext.SaveChangesAsync();
        return Ok(new UpsertExpenseTagResponse{ExpenseTag = expenseTag});
    }

    [HttpPut("expense-tag/{expenseTagId}")]
    public async Task<IActionResult> UpdateExpenseTagAsync(String expenseTagId, [FromBody] ExpenseTag expenseTag)
    {
        var tag = _dbContext.ExpenseTags.Find(expenseTagId);
        if (tag == null) return NotFound();
        
        tag.Name = expenseTag.Name;
        tag.Color = expenseTag.Color;
        tag.Aka = expenseTag.Aka;
        _dbContext.ExpenseTags.Update(tag);
        await _dbContext.SaveChangesAsync();
        
        return Ok(new UpsertExpenseTagResponse{ExpenseTag = expenseTag});
    }

    [HttpDelete("expense-tag/{expenseTagId}")]
    public async Task<IActionResult> DeleteExpenseTagAsync(String expenseTagId)
    {
        var tag = _dbContext.ExpenseTags.Find(expenseTagId);
        if (tag == null) return NotFound();
        _dbContext.ExpenseTags.Remove(tag);
        await _dbContext.SaveChangesAsync();
        
        return Ok("Tag deleted");
    }
}