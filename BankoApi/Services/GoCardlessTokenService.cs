using System.Text.Json;
using DotNetEnv;

namespace BankoApi.Services;

public class GoCardlessTokenService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoCardlessTokenService> _logger;

    private string _accessToken;
    private DateTime _tokenExpiry;

    public GoCardlessTokenService(HttpClient httpClient, IConfiguration configuration,
        ILogger<GoCardlessTokenService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _accessToken = "";
        _tokenExpiry = DateTime.UtcNow;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        // If the token is still valid, return it
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
        {
            _logger.LogInformation("Using cached token.");
            return _accessToken;
        }

        Env.Load();

        // Otherwise, retrieve a new token
        _logger.LogInformation("Retrieving new token from GoCardless.");
        var secretId = Environment.GetEnvironmentVariable("GOCARDLESS_ID") ?? "";
        var secretKey = Environment.GetEnvironmentVariable("GOCARDLESS_KEY") ?? "";

        if (string.IsNullOrEmpty(secretId) || string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("GoCardless credentials are not configured.");

        var response = await _httpClient.PostAsJsonAsync(
            "token/new/",
            new
            {
                secret_id = secretId,
                secret_key = secretKey
            });

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<GoCardlessTokenResponse>(options: new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Access))
            throw new InvalidOperationException("Failed to retrieve GoCardless token.");

        _accessToken = tokenResponse.Access;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.Expires);

        _logger.LogInformation("New token retrieved and cached.");
        return _accessToken;
    }
}

public class GoCardlessTokenResponse
{
    public string Access { get; set; }
    public int Expires { get; set; } // Token expiry time in seconds
}