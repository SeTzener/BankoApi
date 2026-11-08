using BankoApi.Controllers.Devices.Requests;
using BankoApi.Controllers.Devices.Responses;
using BankoApi.Data;
using BankoApi.Data.Dao;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Controllers.Devices;

[ApiController]
[Authorize]
[Route("[controller]")]
public class DevicesController : ControllerBase
{
    private readonly BankoDbContext _dbContext;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(BankoDbContext dbContext, ILogger<DevicesController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Registers or refreshes the FCM token for the current app installation.
    /// Idempotent by token: re-registering updates the owner, platform and last-seen time.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterDeviceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        if (!Enum.TryParse<PushPlatform>(request.Platform, ignoreCase: true, out var platform)
            || !Enum.IsDefined(platform))
        {
            return BadRequest(new { message = "Platform must be Android or Ios" });
        }

        var userId = User.GetUserId();
        var now = DateTime.UtcNow;

        var device = await _dbContext.PushDevices
            .FirstOrDefaultAsync(d => d.Token == request.Token);

        if (device == null)
        {
            device = new PushDevice
            {
                UserId = userId,
                Token = request.Token,
                Platform = platform,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                LastSeenAt = now
            };
            _dbContext.PushDevices.Add(device);
        }
        else
        {
            // The same installation can be signed into a different account, so the token
            // follows the current user instead of staying attached to the previous one.
            device.UserId = userId;
            device.Platform = platform;
            device.IsActive = true;
            device.UpdatedAt = now;
            device.LastSeenAt = now;
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Registered push device {DeviceId} ({Platform}) for user {UserId}",
            device.Id, device.Platform, userId);

        return Ok(new RegisterDeviceResponse
        {
            Id = device.Id,
            Platform = device.Platform,
            IsActive = device.IsActive
        });
    }

    /// <summary>
    /// Stops sending notifications to this installation, e.g. on logout.
    /// Idempotent: unknown or already inactive tokens still return success.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Unregister([FromQuery] string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        var userId = User.GetUserId();
        var device = await _dbContext.PushDevices
            .FirstOrDefaultAsync(d => d.Token == token && d.UserId == userId);

        if (device != null && device.IsActive)
        {
            device.IsActive = false;
            device.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Unregistered push device {DeviceId} for user {UserId}", device.Id, userId);
        }

        return NoContent();
    }
}
