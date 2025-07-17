using EnergyTariffAdvisor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnergyTariffAdvisor.Pages
{
    public class TariffDetailsModel : PageModel
    {
        public TariffBase? Tariff { get; set; }

        public void OnGet()
        {
            Tariff = HttpContext.Session.GetObject<TariffBase>("TariffDetails");
        }
    }
}
