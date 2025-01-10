using System.Net.Http.Headers;
using BankoApi.Data.Dao;

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
    public async Task<List<Transactions>> GetTransactionsAsync(string accountId)
    {
        // TODO(): Manage failures
        var token = await _tokenService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync($"accounts/{accountId}/transactions");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<NordigenTransactionResponse>();
        return result?.Transactions.Select(t => new Transactions
        {
            BankTransactions = t.BankTransactions
        }).ToList() ?? new List<Transactions>();
    }
}

// TODO(): This response has to be written better
public class NordigenTransactionResponse
{
    public List<Transactions> Transactions { get; set; }
}