// response to GET /standard-unit-rates/ with nested DTOs for tariff rates


using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnergyTariffAdvisor.OctopusApi
{
    public class StandardUnitRatesResponse
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("previous")]
        public string? Previous { get; set; }

        [JsonPropertyName("results")]
        public List<StandardUnitRateDto> Results { get; set; } = new();
    }

    public class StandardUnitRateDto
    {
        [JsonPropertyName("value_inc_vat")]
        public decimal ValueIncVat { get; set; }

        [JsonPropertyName("value_exc_vat")]
        public decimal ValueExcVat { get; set; }

        [JsonPropertyName("valid_from")]
        public DateTime ValidFrom { get; set; }

        [JsonPropertyName("valid_to")]
        public DateTime? ValidTo { get; set; }

        [JsonPropertyName("payment_method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [JsonPropertyName("price_availability")]
        public string PriceAvailability { get; set; } = string.Empty;

        [JsonPropertyName("is_variable")]
        public bool IsVariable { get; set; }

        [JsonPropertyName("is_green")]
        public bool IsGreen { get; set; }

        [JsonPropertyName("product_code")]
        public string ProductCode { get; set; } = string.Empty;

        [JsonPropertyName("tariff_code")]
        public string TariffCode { get; set; } = string.Empty;
    }
}