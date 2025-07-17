using Microsoft.AspNetCore.Mvc.RazorPages;
using EnergyTariffAdvisor.OctopusApi;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnergyTariffAdvisor.Pages
{
    public class OctopusTestModel : PageModel
    {
        private readonly OctopusTariffService _octopusService;

        // Список продуктов для отображения
        public List<ProductDto> Products { get; set; } = new();
        public Dictionary<string, List<TariffDetailsDto>> TariffsByProduct { get; set; } = new();
        public Dictionary<string, List<StandardUnitRateDto>> UnitRatesByTariff { get; set; } = new();

        public OctopusTestModel(OctopusTariffService octopusService)
        {
            _octopusService = octopusService;
        }

        public async Task OnGetAsync()
        {
            var response = await _octopusService.GetProductsAsync();
            if (response != null && response.Results != null)
            {
                // 1. Оставляем только продукты с IMPORT и без business
                var filteredProducts = new List<ProductDto>();
                foreach (var product in response.Results)
                {
                    if (product.Direction == "IMPORT" && product.IsBusiness == false)
                    {
                        filteredProducts.Add(product);
                    }
                }

                Products = filteredProducts;

                // 2. Обрабатываем каждый отфильтрованный продукт
                foreach (var product in Products)
                {
                    LinkDto selfLink = null;
                    foreach (var link in product.Links)
                    {
                        if (link.Rel == "self")
                        {
                            selfLink = link;
                            break;
                        }
                    }

                    if (selfLink != null)
                    {
                        var fullProduct = await _octopusService.GetProductDetailsByUrlAsync(selfLink.Href);
                        if (fullProduct != null && fullProduct.SingleRegisterElectricityTariffs != null)
                        {
                            var allTariffs = new List<TariffDetailsDto>();

                            // 3. Перебираем тарифы только для региона "_H"
                            foreach (var regionEntry in fullProduct.SingleRegisterElectricityTariffs)
                            {
                                string regionCode = regionEntry.Key;
                                if (regionCode != "_H")
                                {
                                    continue; // Пропускаем другие регионы
                                }

                                var methodsDict = regionEntry.Value;
                                foreach (var methodEntry in methodsDict)
                                {
                                    string paymentMethod = methodEntry.Key;
                                    var tariff = methodEntry.Value;

                                    allTariffs.Add(tariff);

                                    // 4. Загружаем стандартные ставки по ссылке
                                    if (tariff.Links != null)
                                    {
                                        foreach (var tariffLink in tariff.Links)
                                        {
                                            if (tariffLink.Rel == "standard_unit_rates")
                                            {
                                                var unitRatesResponse = await _octopusService.GetStandardUnitRatesAsync(fullProduct.Code, tariff.Code);
                                                if (unitRatesResponse != null && unitRatesResponse.Results != null)
                                                {
                                                    UnitRatesByTariff[tariff.Code] = unitRatesResponse.Results;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            TariffsByProduct[product.Code] = allTariffs;
                        }
                    }
                }
            }
        }
    }
}