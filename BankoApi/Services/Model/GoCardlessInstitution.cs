using System.Text.Json.Serialization;

namespace BankoApi.Services.Model;

public class GoCardlessInstitution
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("bic")]
    public string Bic { get; set; } = string.Empty;

    [JsonPropertyName("transaction_total_days")]
    public string TransactionTotalDays { get; set; } = string.Empty;

    [JsonPropertyName("countries")]
    public List<string> Countries { get; set; } = new();

    [JsonPropertyName("logo")]
    public string Logo { get; set; } = string.Empty;

    [JsonPropertyName("max_access_valid_for_days")]
    public string MaxAccessValidForDays { get; set; } = string.Empty;
}
