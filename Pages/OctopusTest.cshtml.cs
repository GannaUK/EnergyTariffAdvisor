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

        public OctopusTestModel(OctopusTariffService octopusService)
        {
            _octopusService = octopusService;
        }

        public async Task OnGetAsync()
        {
            var response = await _octopusService.GetProductsAsync();
            if (response != null && response.Results != null)
            {
                Products = response.Results;
                // Iterate over each product to access StandardUnitRateDto
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
                        // Process productDetails as needed
                        if (fullProduct != null && fullProduct.SingleRegisterElectricityTariffs != null)
                        {
                            var allTariffs = new List<TariffDetailsDto>();

                            foreach (var regionEntry in fullProduct.SingleRegisterElectricityTariffs)
                            {
                                // Process each tariff as needed
                                string regionCode = regionEntry.Key;
                                var methodsDict = regionEntry.Value;
                                foreach (var methodEntry in methodsDict)
                                {
                                    string paymentMethod = methodEntry.Key;
                                    var tariff = methodEntry.Value;
                                    
                                    allTariffs.Add(tariff);
                                   
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