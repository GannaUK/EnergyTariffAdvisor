using Microsoft.AspNetCore.Mvc.RazorPages;
using EnergyTariffAdvisor.OctopusApi;

namespace EnergyTariffAdvisor.Pages
{
    public class OctopusTestModel : PageModel
    {
        private readonly OctopusTariffService _octopusService;

        public List<ProductDto> Products { get; set; } = new();

        public OctopusTestModel(OctopusTariffService octopusService)
        {
            _octopusService = octopusService;
        }

        public async Task OnGetAsync()
        {
            var response = await _octopusService.GetProductsAsync();
            if (response != null)
            {
                Products = response.Results;
            }
        }
    }
}
