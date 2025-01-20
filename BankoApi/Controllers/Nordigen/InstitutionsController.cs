using BankoApi.Data;
using BankoApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.Nordigen;

[ApiController]
[Route("[controller]")]
public class InstitutionsController : ControllerBase
{
    private readonly NordigenService _nordigenService;
    private readonly BankoDbContext _dbContext;

    public InstitutionsController(NordigenService nordigenService, BankoDbContext dbContext)
    {
        _nordigenService = nordigenService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetInstitutionsAsync()
    {
        var institutions = await _nordigenService.GetInstitutions();
        foreach (var institution in institutions)
        {
            _dbContext.Institutions.Add(institution);    
        }
        
        await _dbContext.SaveChangesAsync();
        return Ok(institutions);
    }
}

