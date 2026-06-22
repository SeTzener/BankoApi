using System.Net;
using System.Text.Json;
using BankoApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace BankoApi.Tests;

public static class MockHelpers
{
    public static void SetGoCardlessEnvVars()
    {
        Environment.SetEnvironmentVariable("GOCARDLESS_ID", "test-id");
        Environment.SetEnvironmentVariable("GOCARDLESS_KEY", "test-key");
    }

    /// <summary>
    /// Creates an HttpMessageHandler that responds to "token/new/" with a token response
    /// and responds to all other requests with the provided function.
    /// </summary>
    public static Mock<HttpMessageHandler> CreateHandlerWithToken(
        Func<HttpRequestMessage, HttpResponseMessage>? otherRequestsHandler = null)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.AbsolutePath.EndsWith("token/new/")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    access = "test-access-token",
                    expires = 3600
                }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            });

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri != null && !r.RequestUri.AbsolutePath.EndsWith("token/new/")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
                otherRequestsHandler?.Invoke(request) ?? new HttpResponseMessage(HttpStatusCode.OK));

        return handlerMock;
    }

    /// <summary>
    /// Creates a real GoCardlessService that will make HTTP calls through the provided handler.
    /// The handler should handle both token requests and API requests.
    /// </summary>
    public static GoCardlessService CreateGoCardlessServiceWithHandler(HttpMessageHandler handler)
    {
        SetGoCardlessEnvVars();

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.com/v1/")
        };

        var tokenHttpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.com/v1/")
        };

        var configMock = new Mock<IConfiguration>();
        var tokenLoggerMock = new Mock<ILogger<GoCardlessTokenService>>();
        var tokenService = new GoCardlessTokenService(tokenHttpClient, configMock.Object, tokenLoggerMock.Object);

        var serviceLoggerMock = new Mock<ILogger<GoCardlessService>>();
        return new GoCardlessService(httpClient, tokenService, serviceLoggerMock.Object);
    }
}
