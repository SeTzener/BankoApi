using BankoApi.Data.Dao;
using BankoApi.Exceptions.GoCardless.Transactions;
using BankoApi.Services.Model;
using System.Net;
using System.Net.Http.Headers;

namespace BankoApi.Services;

public class GoCardlessService
{
    private const int MaxRetries = 3;
    private static readonly TimeSpan RetryBaseDelay = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(30);
    private static readonly HashSet<HttpStatusCode> TransientStatusCodes = new()
    {
        HttpStatusCode.BadGateway,
        HttpStatusCode.ServiceUnavailable,
        HttpStatusCode.GatewayTimeout,
        HttpStatusCode.TooManyRequests
    };

    private readonly HttpClient _httpClient;
    private readonly GoCardlessTokenService _tokenService;
    private ILogger<GoCardlessService> _logger;

    public GoCardlessService(HttpClient httpClient, GoCardlessTokenService tokenService,
        ILogger<GoCardlessService> logger)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _logger = logger;
    }

    private async Task<HttpResponseMessage> SendWithTransientRetryAsync(Func<Task<HttpResponseMessage>> sendAsync)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            var response = await sendAsync();
            if (!TransientStatusCodes.Contains(response.StatusCode) || attempt == MaxRetries)
                return response;

            var delay = GetRetryDelay(response, attempt);
            _logger.LogWarning("GoCardless returned {StatusCode}; retrying in {Delay} (attempt {Attempt}/{MaxRetries})",
                response.StatusCode, delay, attempt, MaxRetries);
            await Task.Delay(delay);
        }

        throw new InvalidOperationException("Retry loop terminated without returning a response");
    }

    private static TimeSpan GetRetryDelay(HttpResponseMessage response, int attempt)
    {
        if (response.StatusCode == HttpStatusCode.TooManyRequests
            && response.Headers.RetryAfter?.Delta is { } retryAfter)
        {
            return retryAfter > MaxRetryDelay ? MaxRetryDelay : retryAfter;
        }

        return RetryBaseDelay * attempt;
    }

    // TODO():Change this Transactions from DAO to a Model dto
    public async Task<Transactions?> GetTransactionsAsync(Guid accountId)
    {
        // TODO(): Manage failures
        var token = await _tokenService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await SendWithTransientRetryAsync(() => _httpClient.GetAsync($"accounts/{accountId}/transactions/"));

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            String errorResponse = response.Content.ReadAsStringAsync().Result;
            if (errorResponse.Contains(EndUserAgreementExceptionMessages.Message))
            {
                throw new EndUserAgreementException(errorResponse);
            }
        }
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Transactions>();
    }

    public async Task<PaginatedEndUserAgreements> GetEndUserAgreement()
    {
        // TODO: Token MUST be loaded from the DB
        var token = await _tokenService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await SendWithTransientRetryAsync(() => _httpClient.GetAsync("agreements/enduser/"));
        response.EnsureSuccessStatusCode();

        return response.Content.ReadFromJsonAsync<PaginatedEndUserAgreements>().Result ?? new PaginatedEndUserAgreements();
    }

    public async Task<EndUserAgreement> CreateEndUserAgreement(String institutionId, int daysOfAccess)
    {
        // TODO: Token MUST be loaded from the DB
        var token = await _tokenService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.PostAsJsonAsync(
            requestUri: "agreements/enduser/",
            value: new
            {
                institution_id = institutionId, //"BIEN_SPAREBANK_BIENNOK1",
                max_historical_days = daysOfAccess,
                access_valid_for_days = daysOfAccess,
                access_scope = new[] { "balances", "details", "transactions" }
            });

        response.EnsureSuccessStatusCode();

        return response.Content.ReadFromJsonAsync<EndUserAgreement>().Result;
    }

    public async Task<GoCardlessInstitution?> GetInstitutionAsync(string institutionId)
    {
        var token = await _tokenService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await SendWithTransientRetryAsync(() => _httpClient.GetAsync($"institutions/{institutionId}/"));
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GoCardlessInstitution>();
    }

    public async Task<List<GoCardlessInstitution>> GetInstitutionsAsync(string? countryCode = null)
    {
        var token = await _tokenService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var url = countryCode != null ? $"institutions/?country={countryCode}" : "institutions/";
        var response = await SendWithTransientRetryAsync(() => _httpClient.GetAsync(url));
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<GoCardlessInstitution>>() ?? new List<GoCardlessInstitution>();
    }

    public async Task<Model.Requisition> CreateRequisition(string institutionId, string agreementId)
    {

        // TODO: Token MUST be loaded from the DB
        var token = await _tokenService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var formData = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "redirect", "Banko://bank-auth-callback" },
            { "redirect_immediate", "true" },
            { "institution_id", institutionId },
            { "agreement", agreementId },
            { "reference", Guid.NewGuid().ToString() },
            { "user_language", "EN" }
        });

        var response = await _httpClient.PostAsync("requisitions/", formData);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("GoCardless requisition creation failed: {StatusCode} - {Body}", response.StatusCode, errorBody);
            response.EnsureSuccessStatusCode();
        }

        return response.Content.ReadFromJsonAsync<Model.Requisition>().Result;
    }

    public async Task<Model.Requisition> GetRequisitionAsync(string requisitionId)
    {
        var token = await _tokenService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await SendWithTransientRetryAsync(() => _httpClient.GetAsync($"requisitions/{requisitionId}/"));
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Model.Requisition>()
            ?? throw new InvalidOperationException($"Failed to deserialize requisition {requisitionId}");
    }

    public async Task<GoCardlessAccountDetails?> GetAccountDetailsAsync(Guid accountId)
    {
        var token = await _tokenService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await SendWithTransientRetryAsync(() => _httpClient.GetAsync($"accounts/{accountId}/details/"));
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GoCardlessAccountDetails>();
    }
}