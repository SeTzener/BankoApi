using BankoApi.Controllers.Shared;
using BankoApi.Controllers.Users.Requests;
using BankoApi.Controllers.Users.Responses;
using BankoApi.Controllers.Shared.DTO;
using BankoApi.Data;
using BankoApi.Exceptions.User;
using BankoApi.Repository;
using BankoApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}
