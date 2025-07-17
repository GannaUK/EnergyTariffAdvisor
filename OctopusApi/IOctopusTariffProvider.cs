using System.Collections.Generic;
using System.Threading.Tasks;
using EnergyTariffAdvisor.Models;

namespace EnergyTariffAdvisor.OctopusApi
{
    public interface IOctopusTariffProvider
    {
        Task<ProductsResponse?> GetProductsAsync();
        Task<ProductDto?> GetProductDetailsByUrlAsync(string productUrl);
        Task<StandardUnitRatesResponse?> GetStandardUnitRatesAsync(string productCode, string tariffCode, string regionCode = "H");
        Task<StandingChargesResponse?> GetStandingChargesAsync(string productCode, string tariffCode, string regionCode = "H");
    }
}