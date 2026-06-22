using BankoApi.Controllers.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Tests.Utilities;

public class ControllerExtensionsTests
{
    private class TestController : ControllerBase
    {
    }

    [Fact]
    public void Forbidden_WithErrorResponse_ReturnsStatusCode403()
    {
        var controller = new TestController();
        var errorResponse = new ErrorResponse { Message = "Forbidden" };

        var result = controller.Forbidden(errorResponse);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objectResult.StatusCode);
        var response = Assert.IsType<ErrorResponse>(objectResult.Value);
        Assert.Equal("Forbidden", response.Message);
    }

    [Fact]
    public void Forbidden_WithMessage_ReturnsStatusCode403()
    {
        var controller = new TestController();

        var result = controller.Forbidden("Access denied");

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objectResult.StatusCode);
        var response = Assert.IsType<ErrorResponse>(objectResult.Value);
        Assert.Equal("Access denied", response.Message);
    }
}
