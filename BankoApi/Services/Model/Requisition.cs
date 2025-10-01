using System.Text.Json.Serialization;

namespace BankoApi.Services.Model
{
    public class Requisition
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("redirect")]
        public string Redirect { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("institution_id")]
        public string InstitutionId { get; set; } = string.Empty;

        [JsonPropertyName("agreement")]
        public string Agreement { get; set; } = string.Empty;

        [JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;

        [JsonPropertyName("accounts")]
        public List<string> Accounts { get; set; } = new();

        [JsonPropertyName("user_language")]
        public string UserLanguage { get; set; } = string.Empty;

        [JsonPropertyName("link")]
        public string Link { get; set; } = string.Empty;

        [JsonPropertyName("ssn")]
        public string Ssn { get; set; } = string.Empty;

        [JsonPropertyName("account_selection")]
        public bool AccountSelection { get; set; }

        [JsonPropertyName("redirect_immediate")]
        public bool RedirectImmediate { get; set; }
    }
}
