// Класс для работы с API (HttpClient, запросы, преобразования)
// Class for working with the API (HttpClient, requests, transformations)
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EnergyTariffAdvisor.OctopusApi;

namespace EnergyTariffAdvisor.OctopusApi
{
    public class OctopusTariffService
    {
        private readonly HttpClient _httpClient;

        public OctopusTariffService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.octopus.energy/v1/");
        }

        public async Task<ProductsResponse?> GetProductsAsync()
        {
            return await _httpClient.GetFromJsonAsync<ProductsResponse>("products/");
        }

        // Получает полную информацию о продукте по ссылке self
        // Retrieves detailed information about a product by its self link
        public async Task<ProductDto?> GetProductDetailsByUrlAsync(string productUrl)
        {
            return await _httpClient.GetFromJsonAsync<ProductDto>(productUrl);
        }

        // "H" is the region code for North Scotland (including Aberdeen)
        public async Task<StandardUnitRatesResponse?> GetStandardUnitRatesAsync(string productCode, string tariffCode, string regionCode = "H")
        {
            var url = $"products/{productCode}/electricity-tariffs/{tariffCode}/standard-unit-rates/?region={regionCode}";
            return await _httpClient.GetFromJsonAsync<StandardUnitRatesResponse>(url);
        }

        public async Task<StandingChargesResponse?> GetStandingChargesAsync(string productCode, string tariffCode, string regionCode = "H")
        {
            var url = $"products/{productCode}/electricity-tariffs/{tariffCode}/standing-charges/?region={regionCode}";
            return await _httpClient.GetFromJsonAsync<StandingChargesResponse>(url);
        }
    }
}