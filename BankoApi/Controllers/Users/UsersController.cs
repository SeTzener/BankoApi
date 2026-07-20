using BankoApi.Controllers.Shared;
using BankoApi.Controllers.Users.Requests;
using BankoApi.Controllers.Users.Responses;
using BankoApi.Controllers.Shared.DTO;
using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Exceptions.User;
using BankoApi.Repository;
using BankoApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BankoApi.Controllers.Users;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly BankoDbContext _dbContext;
    private readonly UserRepository _repository;
    private readonly TokenService _tokenService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        BankoDbContext dbContext,
        UserRepository repository,
        TokenService tokenService,
        ILogger<UsersController> logger)
    {
        _dbContext = dbContext;
        _repository = repository;
        _tokenService = tokenService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> NewUser([FromBody] NewUserRequest request)
    {
        try
        {
            var userId = _repository.CreateAccount(
                userData: new UserDto
                {
                    Email = request.Email,
                    Password = request.Password,
                    FullName = request.FullName,
                    Address = request.Address,
                    PhoneNumber = request.PhoneNumber,
                    ConsentGiven = request.ConsentGiven
                },
                context: _dbContext
            );
            await _dbContext.SaveChangesAsync();

            var user = _dbContext.Users.Find(userId)!;
            var (accessToken, refreshToken, expiresIn) = _tokenService.GenerateTokens(user);

            return Ok(new UserResponse
            {
                AccountId = userId,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = expiresIn
            });
        }
        catch (EmailConflictException ex)
        {
            _logger.LogError(ex, "Email conflict");
            return Conflict(new ErrorResponse
            {
                Message = UserErrorMessages.EmailAlreadyExists.ToString()
            });
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var userId = _repository.ValidateAccount(_dbContext, request.Email, request.Password);

            var user = _dbContext.Users.Find(userId)!;
            var (accessToken, refreshToken, expiresIn) = _tokenService.GenerateTokens(user);

            return Ok(new UserResponse
            {
                AccountId = userId,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = expiresIn
            });
        }
        catch (Exception ex) when (ex is EmailNotFoundException || ex is PasswordNotFoundException)
        {
            _logger.LogError(ex, "Wrong credentials");
            return Unauthorized(new ErrorResponse
            {
                Message = UserErrorMessages.WrongCredentials.ToString()
            });
        }
        catch (InactiveUserException ex)
        {
            _logger.LogError(ex, "Inactive account");
            return this.Forbidden(new ErrorResponse
            {
                Message = UserErrorMessages.InactiveAccount.ToString()
            });
        }
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var (userId, accessToken, refreshToken, expiresIn) =
                await _tokenService.RefreshTokenAsync(request.RefreshToken);

            return Ok(new UserResponse
            {
                AccountId = userId,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = expiresIn
            });
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return Unauthorized(new ErrorResponse
            {
                Message = UserErrorMessages.SomethingWentWrong.ToString()
            });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        return Ok();
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetProfile()
    {
        var userId = User.GetUserId();
        var user = _dbContext.Users.Find(userId);
        if (user == null)
            return NotFound(new ErrorResponse { Message = "User not found" });

        return Ok(new UserProfileResponse
        {
            AccountId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            ConsentGiven = user.ConsentGiven,
            ConsentUpdatedAt = user.ConsentUpdatedAt,
            ConsentVersionId = user.ConsentVersionId,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt
        });
    }

    [Authorize]
    [HttpPut("me")]
    public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.GetUserId();
        var user = _dbContext.Users.Find(userId);
        if (user == null)
            return NotFound(new ErrorResponse { Message = "User not found" });

        if (request.FullName != null) user.FullName = request.FullName;
        if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;
        if (request.Address != null) user.Address = request.Address;

        user.UpdatedAt = DateTime.UtcNow;
        _dbContext.SaveChanges();

        return Ok(new UserProfileResponse
        {
            AccountId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            ConsentGiven = user.ConsentGiven,
            ConsentUpdatedAt = user.ConsentUpdatedAt,
            ConsentVersionId = user.ConsentVersionId,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt
        });
    }

    [Authorize]
    [HttpPut("me/password")]
    public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.GetUserId();
        try
        {
            _repository.UpdatePassword(_dbContext, userId, request.CurrentPassword, request.NewPassword);
            return Ok();
        }
        catch (PasswordNotFoundException ex)
        {
            _logger.LogError(ex, "Wrong password");
            return Unauthorized(new ErrorResponse
            {
                Message = UserErrorMessages.WrongCredentials.ToString()
            });
        }
        catch (EmailNotFoundException ex)
        {
            _logger.LogError(ex, "User not found");
            return NotFound(new ErrorResponse { Message = "User not found" });
        }
    }

    [Authorize]
    [HttpPut("me/consent")]
    public IActionResult AcceptConsent([FromBody] AcceptConsentRequest request)
    {
        var userId = User.GetUserId();
        var user = _dbContext.Users.Find(userId);
        if (user == null)
            return NotFound(new ErrorResponse { Message = "User not found" });

        var policyVersion = _dbContext.PrivacyPolicyVersions.Find(request.PolicyVersionId);
        if (policyVersion == null)
            return NotFound(new ErrorResponse { Message = "Privacy policy version not found" });

        var consentLog = new ConsentLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PrivacyPolicyVersionId = request.PolicyVersionId,
            Accepted = true,
            RecordedAt = DateTime.UtcNow
        };
        _dbContext.ConsentLogs.Add(consentLog);

        user.ConsentGiven = true;
        user.ConsentVersionId = request.PolicyVersionId;
        user.ConsentUpdatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        _dbContext.SaveChanges();

        return Ok();
    }

    [Authorize]
    [HttpGet("me/export")]
    public IActionResult ExportData()
    {
        var userId = User.GetUserId();
        var user = _dbContext.Users.Find(userId);
        if (user == null)
            return NotFound(new ErrorResponse { Message = "User not found" });

        var consentLogs = _dbContext.ConsentLogs
            .Where(cl => cl.UserId == userId)
            .AsNoTracking()
            .Include(cl => cl.PrivacyPolicyVersion)
            .Select(cl => new
            {
                PolicyVersion = cl.PrivacyPolicyVersion.Version,
                PolicyTitle = cl.PrivacyPolicyVersion.Title,
                Accepted = cl.Accepted,
                RecordedAt = cl.RecordedAt
            })
            .ToList();

        var export = new
        {
            AccountId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            ConsentGiven = user.ConsentGiven,
            ConsentUpdatedAt = user.ConsentUpdatedAt,
            ConsentVersionId = user.ConsentVersionId,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            ConsentLogs = consentLogs
        };

        return Ok(export);
    }

    [Authorize]
    [HttpDelete("me")]
    public IActionResult DeleteAccount()
    {
        var userId = User.GetUserId();
        try
        {
            _repository.SoftDeleteUser(_dbContext, userId);
            return Ok();
        }
        catch (EmailNotFoundException ex)
        {
            _logger.LogError(ex, "User not found");
            return NotFound(new ErrorResponse { Message = "User not found" });
        }
    }
}
