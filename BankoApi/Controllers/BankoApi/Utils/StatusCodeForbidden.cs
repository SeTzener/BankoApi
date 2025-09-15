using BankoApi.Controllers.BankoApi.Controllers.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.BankoApi.Utils;

public static class ControllerExtensions
{
    public static IActionResult Forbidden(this ControllerBase controller, ErrorResponse errorResponse)
    {
        return new ObjectResult(errorResponse)
        {
            StatusCode = 403
        };
    }

    // Optional overload for convenience
    public static IActionResult Forbidden(this ControllerBase controller, string message)
    {
        return controller.Forbidden(new ErrorResponse { Message = message });
    }
}