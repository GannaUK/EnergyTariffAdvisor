using EnergyTariffAdvisor.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace EnergyTariffAdvisor.Pages
{
    public class TestTariffModel : PageModel
    {
        public decimal Cost { get; set; }

        public void OnGet()
        {
            var profile = new HalfHourlyConsumptionProfile
            {
                Consumption = new List<decimal>()
            };

            // Наполним профайл потребления 48 значениями (по 0.5 кВт·ч)
            for (int i = 0; i < 48; i++)
            {
                profile.Consumption.Add(0.5m);
            }

            var tariff = new FixedTariff
            {
                TariffCode = "TEST-001",
                TariffName = "Fixed Demo",
                ProductName = "Test Product",
                SupplierName = "Test Energy",
                Description = "Demo Fixed Tariff",
                TariffType = TariffType.Fixed,
                UnitRate = 15.0m, // 15p/kWh
                StandingChargeDaily = 45.0m // 45p/day
            };

            Cost = tariff.CalculateCost(profile); // должен вернуть: (0.5 * 48 * 15) + 45 = 405 + 45 = 450p = £4.50
        }
    }
}