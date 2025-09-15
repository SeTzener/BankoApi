using BankoApi.Controllers.BankoApi.Controllers.Responses;
using BankoApi.Controllers.BankoApi.Controllers.UsersController.Requests;
using BankoApi.Controllers.BankoApi.Controllers.UsersController.Responses;
using BankoApi.Controllers.BankoApi.DTO;
using BankoApi.Controllers.BankoApi.Utils;
using BankoApi.Data;
using BankoApi.Exceptions.User;
using BankoApi.Repository;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.BankoApi.Controllers.UsersController;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly BankoDbContext _dbContext;
    private readonly UserRepository _repository;

    public UsersController(BankoDbContext dbContext)
    {
        _dbContext = dbContext;
        _repository = new UserRepository();
    }

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
            return Ok(new UserResponse
            {
                AccountId = userId,
                AccessToken = "ACCESS_TOKEN",
                RefreshToken = "REFRESH_TOKEN",
                ExpiresIn = 1234567890
            });
        }
        catch (EmailConflictException ex)
        {
            Console.WriteLine(ex.Message);
            return Conflict(new ErrorResponse
            {
                Message = UserErrorMessages.EmailAlreadyExists.ToString()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest(new ErrorResponse
            {
                Message = UserErrorMessages.SomethingWentWrong.ToString()
            });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var userId = _repository.ValidateAccount(_dbContext, request.Email, request.Password);

            return Ok(new UserResponse
            {
                AccountId = userId,
                AccessToken = "ACCESS_TOKEN",
                RefreshToken = "REFRESH_TOKEN",
                ExpiresIn = 1234567890
            });
        }
        catch (Exception ex) when (ex is EmailNotFoundException || ex is PasswordNotFoundException)
        {
            Console.WriteLine(ex.Message);
            return Unauthorized(new ErrorResponse
            {
                Message = UserErrorMessages.WrongCredentials.ToString()
            });
        }
        catch (InactiveUserException ex)
        {
            Console.WriteLine(ex.Message);
            return this.Forbidden(new ErrorResponse
            {
                Message = UserErrorMessages.InactiveAccount.ToString()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest(new ErrorResponse
            {
                Message = UserErrorMessages.SomethingWentWrong.ToString()
            });
        }
    }
}