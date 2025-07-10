using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

// Ответ на GET /products/ с вложенными DTO (ProductDto, LinkDto и т.д.)
// response of GET /products/ with nested DTOs (ProductDto, LinkDto, etc.)

namespace EnergyTariffAdvisor.OctopusApi
{
    public class ProductsResponse
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("previous")]
        public string? Previous { get; set; }

        [JsonPropertyName("results")]
        public List<ProductDto> Results { get; set; } = new();
    }

    public class ProductDto
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("direction")]
        public string Direction { get; set; } = string.Empty;

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("is_variable")]
        public bool IsVariable { get; set; }

        [JsonPropertyName("is_green")]
        public bool IsGreen { get; set; }

        [JsonPropertyName("is_tracker")]
        public bool IsTracker { get; set; }

        [JsonPropertyName("is_prepay")]
        public bool IsPrepay { get; set; }

        [JsonPropertyName("is_business")]
        public bool IsBusiness { get; set; }

        [JsonPropertyName("is_restricted")]
        public bool IsRestricted { get; set; }

        [JsonPropertyName("term")]
        public int? Term { get; set; }

        [JsonPropertyName("available_from")]
        public DateTime AvailableFrom { get; set; }

        [JsonPropertyName("available_to")]
        public DateTime? AvailableTo { get; set; }

        [JsonPropertyName("links")]
        public List<LinkDto> Links { get; set; } = new();

        [JsonPropertyName("brand")]
        public string Brand { get; set; } = string.Empty;
    }

    public class LinkDto
    {
        [JsonPropertyName("href")]
        public string Href { get; set; } = string.Empty;

        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        [JsonPropertyName("rel")]
        public string Rel { get; set; } = string.Empty;
    }
}
