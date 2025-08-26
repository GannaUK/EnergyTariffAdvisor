
// Class for working with the API (HttpClient, requests, transformations)
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EnergyTariffAdvisor.OctopusApi;

namespace EnergyTariffAdvisor.OctopusApi
{
    public class OctopusTariffService : IOctopusTariffProvider
    {
        private readonly HttpClient _httpClient;

        // constructor
        public OctopusTariffService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.octopus.energy/v1/");
        }

        // step 1 - Retrieves all products https://api.octopus.energy/v1/products/
        public async Task<ProductsResponse?> GetProductsAsync()
        {
            return await _httpClient.GetFromJsonAsync<ProductsResponse>("products/");
        }
        // step 2 - Retrieves product details and tariffs via self link, e.g. https://api.octopus.energy/v1/products/E-1R-AGILE-24-10-01-A
        public async Task<ProductDto?> GetProductDetailsByUrlAsync(string productUrl)
        {
            return await _httpClient.GetFromJsonAsync<ProductDto>(productUrl);
        }

        // step 3 - Retrieves unit rates for a specific tariff, e.g. https://api.octopus.energy/v1/products/AGILE-24-10-01/electricity-tariffs/E-1R-AGILE-24-10-01-A/standard-unit-rates/
        // "H" is the region code for North Scotland (including Aberdeen)
        public async Task<StandardUnitRatesResponse?> GetStandardUnitRatesAsync(string productCode, string tariffCode, string regionCode = "H")
        {
            
            // here is a workaround - in the Octopus Energy API, it is not always possible to get tariffs for the current day, so we request tariffs for yesterday
            //var todayUtc = DateTime.UtcNow.Date; // 00:00 of current day UTC
            //var tomorrowUtc = todayUtc.AddDays(1); // 00:00 of next day
            var todayUtc = DateTime.UtcNow.Date.AddDays(-1); // 00:00 вчера UTC
            var tomorrowUtc = DateTime.UtcNow.Date; // 00:00 of current day UTC

            var periodFrom = todayUtc.ToString("yyyy-MM-ddTHH:mm:ssZ");    // e.g. 2025-07-16T00:00:00Z
            var periodTo = tomorrowUtc.ToString("yyyy-MM-ddTHH:mm:ssZ");  // e.g. 2025-07-17T00:00:00Z

            var url = $"products/{productCode}/electricity-tariffs/{tariffCode}/standard-unit-rates/?" +
                      $"region={regionCode}&period_from={periodFrom}&period_to={periodTo}";

            return await _httpClient.GetFromJsonAsync<StandardUnitRatesResponse>(url);
        }

        // step 3 - Retrieves standing charges for a specific tariff, e.g. https://api.octopus.energy/v1/products/AGILE-24-10-01/electricity-tariffs/E-1R-AGILE-24-10-01-A/standing-charges/
        // "H" is the region code for North Scotland (including Aberdeen)
        public async Task<StandingChargesResponse?> GetStandingChargesAsync(string productCode, string tariffCode, string regionCode = "H")
        {
            var url = $"products/{productCode}/electricity-tariffs/{tariffCode}/standing-charges/?region={regionCode}";
            return await _httpClient.GetFromJsonAsync<StandingChargesResponse>(url);
        }
    }
}