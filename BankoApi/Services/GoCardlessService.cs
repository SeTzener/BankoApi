using BankoApi.Controllers.GoCardless.Responses;
using BankoApi.Data.Dao;
using BankoApi.Exceptions.GoCardless.Transactions;
using BankoApi.Services.Model;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace BankoApi.Services;

public class GoCardlessService
{
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

    // TODO():Change this Transactions from DAO to a Model dto
    public async Task<Transactions?> GetTransactionsAsync(string accountId)
    {
        // TODO(): Manage failures
        var token = await _tokenService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync($"accounts/{accountId}/transactions/");

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

    private string FindAgreementId(string input)
    {
        string pattern = @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
        Match match = Regex.Match(input, pattern);

        return match.Success ? match.Value : throw new EndUserAgreementException(FetchAndStoreTransactionResponse.AgreementIdNotFound.ToString());
    }

    public async Task<List<Institutions>> GetInstitutions()
    {
        var token = await _tokenService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync("institutions/?country=it");
        response.EnsureSuccessStatusCode();
        return response.Content.ReadFromJsonAsync<List<Institutions>>().Result ?? new List<Institutions>();
    }

    public async Task<PaginatedEndUserAgreements> GetEndUserAgreement()
    {
        // TODO: Token MUST be loaded from the DB
        var token = await _tokenService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync("agreements/enduser/");
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

    public async Task<Model.Requisition> CreateRequisition(string institutionId, string agreementId)
    {

        // TODO: Token MUST be loaded from the DB
        var token = await _tokenService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.PostAsJsonAsync(
            requestUri: "requisitions/",
            value: new
            {
                redirect = "Banko://bank-auth-callback",
                institution_id = institutionId,
                agreement = agreementId,
                reference = Guid.NewGuid().ToString(),
                user_language = "EN"
            });
        response.EnsureSuccessStatusCode();

        return response.Content.ReadFromJsonAsync<Model.Requisition>().Result;
    }
}