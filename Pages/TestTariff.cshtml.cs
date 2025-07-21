using EnergyTariffAdvisor.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Text.Json;

namespace EnergyTariffAdvisor.Pages
{
    public class TestTariffModel : PageModel
    {
        public decimal Cost { get; set; }

        public HalfHourlyConsumptionProfile Profile { get; set; }

        public void OnGet()
        {
            //Profile = new HalfHourlyConsumptionProfile
            //{
            //    Consumption = new List<decimal>()
            //};

            Profile = HttpContext.Session.GetObject<HalfHourlyConsumptionProfile>("UserProfile");

            // Если данных нет — создаём заглушку
            if (Profile == null)
            {
                Profile = new HalfHourlyConsumptionProfile();
                for (int i = 0; i < 48; i++)
                {
                    Profile.Consumption.Add(0.5m); // или 0
                }
            }
            // Наполним профайл потребления 48 значениями (по 0.5 кВт·ч)
            //for (int i = 0; i < 48; i++)
            //{
            //    Profile.Consumption.Add(0.5m);
            //}

            var tariff = new FixedTariff(0.2579m,0.5869m)
            {
                TariffCode = "SCOT-NORTH-FIXED-2025",
                //TariffName = "North Scotland Default Tariff",
                ProductName = "Default Tariff Cap",
                SupplierName = "Generic Supplier",
                Description = "Based on Ofgem price cap for North Scotland",
                TariffType = TariffType.Fixed
                //StandingChargeDaily = 0.5869m, // в фунтах
                //UnitRate = 0.2579m       // в фунтах
            };

            Cost = tariff.CalculateCost(Profile); 
        }
    }
}