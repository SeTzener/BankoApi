namespace BankoApi.Services;

public class NordigenTokenService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NordigenTokenService> _logger;

    private string _accessToken;
    private DateTime _tokenExpiry;

    public NordigenTokenService(HttpClient httpClient, IConfiguration configuration, ILogger<NordigenTokenService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        // If the token is still valid, return it
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
        {
            _logger.LogInformation("Using cached token.");
            return _accessToken;
        }

        // Otherwise, retrieve a new token
        _logger.LogInformation("Retrieving new token from Nordigen.");
        var secretId = _configuration["NordigenAPI:SecretId"];
        var secretKey = _configuration["NordigenAPI:SecretKey"];

        if (string.IsNullOrEmpty(secretId) || string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("Nordigen credentials are not configured.");
        }

        var response = await _httpClient.PostAsJsonAsync("token/new/", new
        {
            secret_id = secretId,
            secret_key = secretKey
        });

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<NordigenTokenResponse>();
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Access))
        {
            throw new InvalidOperationException("Failed to retrieve Nordigen token.");
        }

        _accessToken = tokenResponse.Access;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.Expires);

        _logger.LogInformation("New token retrieved and cached.");
        return _accessToken;
    }
}

public class NordigenTokenResponse
{
    public string Access { get; set; }
    public int Expires { get; set; } // Token expiry time in seconds
}
