using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Repository;
using BankoApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.Settings;

[ApiController]
[Authorize]
[Route("[controller]")]
public partial class SettingsController : ControllerBase
{
    private readonly BankoDbContext _dbContext;
    private readonly GoCardlessService _goCardlessService;
    private readonly BankAuthorizationRepository _repository;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(GoCardlessService goCardlessService, BankoDbContext dbContext, BankAuthorizationRepository repository, ILogger<SettingsController> logger)
    {
        _goCardlessService = goCardlessService;
        _dbContext = dbContext;
        _repository = repository;
        _logger = logger;
    }
}
