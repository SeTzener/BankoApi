using BankoApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.BankoApi
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly BankoDbContext _dbContext;

        public AccountController(BankoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public Task<IActionResult> Index()
        {
            return View();
        }
    }
}
