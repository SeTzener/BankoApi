using System.Net;
using System.Text.Json;
using BankoApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace BankoApi.Tests.Services;

public class GoCardlessTokenServiceTests
{
    private Mock<HttpMessageHandler> CreateMessageHandlerMock(HttpStatusCode statusCode, object? responseContent)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(
                    responseContent != null
                        ? JsonSerializer.Serialize(responseContent, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        })
                        : "")
            });
        return handlerMock;
    }

    private GoCardlessTokenService CreateService(HttpMessageHandler handler, string? secretId = null, string? secretKey = null)
    {
        Environment.SetEnvironmentVariable("GOCARDLESS_ID", secretId ?? "test-id");
        Environment.SetEnvironmentVariable("GOCARDLESS_KEY", secretKey ?? "test-key");

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.com/v1/")
        };

        var configMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<GoCardlessTokenService>>();

        return new GoCardlessTokenService(httpClient, configMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetAccessTokenAsync_NoCachedToken_RetrievesNewToken()
    {
        var tokenResponse = new
        {
            access = "new-access-token",
            expires = 3600
        };
        var handlerMock = CreateMessageHandlerMock(HttpStatusCode.OK, tokenResponse);
        var service = CreateService(handlerMock.Object);

        var token = await service.GetAccessTokenAsync();

        Assert.Equal("new-access-token", token);
    }

    [Fact]
    public async Task GetAccessTokenAsync_CachedValidToken_ReturnsCachedToken()
    {
        var tokenResponse = new
        {
            access = "first-token",
            expires = 3600
        };
        var handlerMock = CreateMessageHandlerMock(HttpStatusCode.OK, tokenResponse);
        var service = CreateService(handlerMock.Object);

        var firstToken = await service.GetAccessTokenAsync();
        var secondToken = await service.GetAccessTokenAsync();

        Assert.Equal("first-token", firstToken);
        Assert.Equal("first-token", secondToken);

        // Verify the handler was called only once (cached)
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetAccessTokenAsync_MissingCredentials_ThrowsInvalidOperationException()
    {
        Environment.SetEnvironmentVariable("GOCARDLESS_ID", "");
        Environment.SetEnvironmentVariable("GOCARDLESS_KEY", "");

        var handlerMock = CreateMessageHandlerMock(HttpStatusCode.OK, new { });
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.example.com/v1/")
        };
        var configMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<GoCardlessTokenService>>();
        var service = new GoCardlessTokenService(httpClient, configMock.Object, loggerMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetAccessTokenAsync());
    }

    [Fact]
    public async Task GetAccessTokenAsync_ApiReturnsNullResponse_ThrowsInvalidOperationException()
    {
        var handlerMock = CreateMessageHandlerMock(HttpStatusCode.OK, new { access = (string?)null, expires = 0 });
        var service = CreateService(handlerMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetAccessTokenAsync());
    }
}
