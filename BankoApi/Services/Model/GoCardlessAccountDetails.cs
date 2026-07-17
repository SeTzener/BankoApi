using System.Text.Json.Serialization;

namespace BankoApi.Services.Model
{
    public class GoCardlessAccountDetails
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("iban")]
        public string? Iban { get; set; }

        [JsonPropertyName("bban")]
        public string? Bban { get; set; }

        [JsonPropertyName("bic")]
        public string? Bic { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("owner_name")]
        public string? OwnerName { get; set; }

        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("product")]
        public string? Product { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("usage")]
        public string? Usage { get; set; }
    }
}
