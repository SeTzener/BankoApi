using BankoApi.Controllers.BankoApi.Controllers.SettingsController.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.BankoApi.SettingsController
{
    public partial class SettingsController
    {
        [HttpGet("Bankinfo")]
        public async Task<IActionResult> GetBankInfoAsync()
        {

            return Ok(new GetBankInfoResponse
            {
            });
        }
    }
}
