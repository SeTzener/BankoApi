using BankoApi.Data;
using BankoApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.GoCardless;

[ApiController]
[Route("[controller]")]
public class InstitutionsController : ControllerBase
{
    private readonly BankoDbContext _dbContext;
    private readonly GoCardlessService _goCardlessService;

    public InstitutionsController(GoCardlessService goCardlessService, BankoDbContext dbContext)
    {
        _goCardlessService = goCardlessService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> FetchInstitutionsAsync()
    {
        var institutions = await _goCardlessService.GetInstitutions();
        foreach (var institution in institutions) _dbContext.Institutions.Add(institution);

        await _dbContext.SaveChangesAsync();
        return Ok(institutions);
    }
}