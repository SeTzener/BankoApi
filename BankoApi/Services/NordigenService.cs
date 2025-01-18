using System.Net.Http.Headers;
using BankoApi.Data.Dao;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BankoApi.Services;

public class NordigenService
{
    private HttpClient _httpClient;
    private NordigenTokenService _tokenService;
    private ILogger<NordigenService> _logger;
    
    public NordigenService(HttpClient httpClient, NordigenTokenService tokenService, ILogger<NordigenService> logger)
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
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Transactions>();
    }

    public async Task<List<Institutions>> GetInstitutions()
    {
        var token = await _tokenService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync($"institutions/?country=it");
        response.EnsureSuccessStatusCode();
        return response.Content.ReadFromJsonAsync<List<Institutions>>().Result ?? new List<Institutions>();
    }
}