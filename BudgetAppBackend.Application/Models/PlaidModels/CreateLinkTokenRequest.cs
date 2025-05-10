using System.Text.Json.Serialization;

namespace BudgetAppBackend.Application.Models.PlaidModels
{
    public class CreateLinkTokenRequest
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } = string.Empty;

        [JsonPropertyName("secret")]
        public string Secret { get; set; } = string.Empty;

        [JsonPropertyName("client_name")]
        public string ClientName { get; set; } = string.Empty;

        [JsonPropertyName("user")]
        public PlaidUser User { get; set; } = new();

        [JsonPropertyName("products")]
        public List<string> Products { get; set; } = new();

        [JsonPropertyName("country_codes")]
        public List<string> CountryCodes { get; set; } = new();

        [JsonPropertyName("language")]
        public string Language { get; set; } = "en";

        [JsonPropertyName("account_filters")]
        public object AccountFilters { get; set; }
    }

    public class PlaidUser
    {
        [JsonPropertyName("client_user_id")]
        public string ClientUserId { get; set; } = string.Empty;
    }
}
