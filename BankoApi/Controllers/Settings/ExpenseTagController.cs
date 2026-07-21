using BankoApi.Controllers.Settings.Responses;
using BankoApi.Data.Dao;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Controllers.Settings
{
    public partial class SettingsController
    {
        [HttpGet("expense-tags")]
        public async Task<IActionResult> GetAllTagsAsync()
        {
            var userId = User.GetUserId();
            return Ok(new GetExpenseTagsResponse
            {
                ExpenseTags = _dbContext.ExpenseTag.AsNoTracking().Where(t => t.UserId == userId).ToList()
            });
        }

        [HttpPost("expense-tag")]
        public async Task<IActionResult> AddExpenseTagAsync([FromBody] ExpenseTag expenseTag)
        {
            var userId = User.GetUserId();
            expenseTag.UserId = userId;
            _dbContext.ExpenseTag.Add(expenseTag);
            await _dbContext.SaveChangesAsync();
            return Ok(new UpsertExpenseTagResponse { ExpenseTag = expenseTag });
        }

        [HttpPut("expense-tag/{expenseTagId}")]
        public async Task<IActionResult> UpdateExpenseTagAsync(string expenseTagId, [FromBody] ExpenseTag expenseTag)
        {
            var userId = User.GetUserId();
            var tag = _dbContext.ExpenseTag.Find(expenseTagId);
            if (tag == null) return NotFound();
            if (tag.UserId != userId) return Forbid();

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
            var userId = User.GetUserId();
            var tag = _dbContext.ExpenseTag.Find(expenseTagId);
            if (tag == null) return NotFound();
            if (tag.UserId != userId) return Forbid();
            _dbContext.ExpenseTag.Remove(tag);
            await _dbContext.SaveChangesAsync();

            return Ok("Tag deleted");
        }
    }
}
