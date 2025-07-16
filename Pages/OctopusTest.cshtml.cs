using Microsoft.AspNetCore.Mvc.RazorPages;
using EnergyTariffAdvisor.OctopusApi;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnergyTariffAdvisor.Pages
{
    public class OctopusTestModel : PageModel
    {
        private readonly OctopusTariffService _octopusService;

        // ������ ��������� ��� �����������
        public List<ProductDto> Products { get; set; } = new();
        public Dictionary<string, List<TariffDetailsDto>> TariffsByProduct { get; set; } = new();
        public Dictionary<string, List<StandardUnitRateDto>> UnitRatesByTariff { get; set; } = new();

        public OctopusTestModel(OctopusTariffService octopusService)
        {
            _octopusService = octopusService;
        }

        public async Task OnGetAsync()
        {
            // ��� 1: �������� ������ ���� ��������� �� Octopus Energy
            var response = await _octopusService.GetProductsAsync();
            if (response != null && response.Results != null)
            {
                Products = response.Results;

                // �������������� �������, ����� �������� ������ ��������� ��������
                TariffsByProduct = new Dictionary<string, List<TariffDetailsDto>>();
                UnitRatesByTariff = new Dictionary<string, List<StandardUnitRateDto>>();

                // ��� 2: ��� ������� �������� ��������� ��������� ����������
                foreach (var product in Products)
                {
                    // ���� ������ � rel="self", ����� �������� ������ ������ � ��������
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

                        // ��� 3: ��������� ������ �� fullProduct
                        if (fullProduct != null && fullProduct.SingleRegisterElectricityTariffs != null)
                        {
                            var allTariffs = new List<TariffDetailsDto>();

                            // �������� �� �������� (��������, "_A", "_B" � �.�.)
                            foreach (var regionEntry in fullProduct.SingleRegisterElectricityTariffs)
                            {
                                var methodsDict = regionEntry.Value;

                                // �������� �� �������� ������ (��������, "direct_debit_monthly")
                                foreach (var methodEntry in methodsDict)
                                {
                                    var tariff = methodEntry.Value;
                                    allTariffs.Add(tariff);

                                    // ��� 4: ��������� ������ (standard-unit-rates) �� ������ �� tariff.Links
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

                            // ��� 5: ��������� ��� ������, ��������� � ���������
                            TariffsByProduct[product.Code] = allTariffs;
                        }
                    }
                }
            }
        }
    }
}