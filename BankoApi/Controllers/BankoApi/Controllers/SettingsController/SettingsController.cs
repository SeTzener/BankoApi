using BankoApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.BankoApi.SettingsController;

[ApiController]
[Route("[controller]")]
public partial class SettingsController : ControllerBase
{
    private readonly BankoDbContext _dbContext;

    public SettingsController(BankoDbContext dbContext)
    {
        _dbContext = dbContext;
    }
}