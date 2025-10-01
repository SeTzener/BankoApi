using System.Text.Json.Serialization;

namespace BankoApi.Services.Model
{
    public class PaginatedEndUserAgreements
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("previous")]
        public string? Previous { get; set; }

        [JsonPropertyName("results")]
        public List<EndUserAgreement> Results { get; set; } = new();
    }
    public class EndUserAgreement
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("institution_id")]
        public string InstitutionId { get; set; } = string.Empty;

        [JsonPropertyName("max_historical_days")]
        public int MaxHistoricalDays { get; set; }

        [JsonPropertyName("access_valid_for_days")]
        public int AccessValidForDays { get; set; }

        [JsonPropertyName("access_scope")]
        public List<string> AccessScope { get; set; } = new();

        [JsonPropertyName("accepted")]
        public DateTime? Accepted { get; set; }

        [JsonPropertyName("reconfirmation")]
        public bool Reconfirmation { get; set; }
    }
}
