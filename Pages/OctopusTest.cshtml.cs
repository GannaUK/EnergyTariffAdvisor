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
            // Шаг 1: Получаем список всех продуктов от Octopus Energy
            var response = await _octopusService.GetProductsAsync();
            if (response != null && response.Results != null)
            {
                Products = response.Results;

                // Инициализируем словари, чтобы избежать ошибок повторной загрузки
                TariffsByProduct = new Dictionary<string, List<TariffDetailsDto>>();
                UnitRatesByTariff = new Dictionary<string, List<StandardUnitRateDto>>();

                // Шаг 2: Для каждого продукта загружаем детальную информацию
                foreach (var product in Products)
                {
                    // Ищем ссылку с rel="self", чтобы получить полные данные о продукте
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

                        // Шаг 3: Извлекаем тарифы из fullProduct
                        if (fullProduct != null && fullProduct.SingleRegisterElectricityTariffs != null)
                        {
                            var allTariffs = new List<TariffDetailsDto>();

                            // Проходим по регионам (например, "_A", "_B" и т.п.)
                            foreach (var regionEntry in fullProduct.SingleRegisterElectricityTariffs)
                            {
                                var methodsDict = regionEntry.Value;

                                // Проходим по способам оплаты (например, "direct_debit_monthly")
                                foreach (var methodEntry in methodsDict)
                                {
                                    var tariff = methodEntry.Value;
                                    allTariffs.Add(tariff);

                                    // Шаг 4: Загружаем ставки (standard-unit-rates) по ссылке из tariff.Links
                                    if (tariff.Links != null)
                                    {
                                        foreach (var link in tariff.Links)
                                        {
                                            if (link.Rel == "standard_unit_rates")
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

                            // Шаг 5: Сохраняем все тарифы, связанные с продуктом
                            TariffsByProduct[product.Code] = allTariffs;
                        }
                    }
                }
            }
        }
    }
}