using Microsoft.AspNetCore.Mvc.RazorPages;
using EnergyTariffAdvisor.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace EnergyTariffAdvisor.Pages
{
    public class ComparisonResultsModel : PageModel
    {
        public HalfHourlyConsumptionProfile Profile { get; set; }
        public List<TariffCostResult> Results { get; set; } = new List<TariffCostResult>();

        public void OnGet()
        {
            Profile = HttpContext.Session.GetObject<HalfHourlyConsumptionProfile>("UserProfile");
            //var tariffs = HttpContext.Session.GetObject<List<TariffBase>>("AvailableTariffs");
            var tariffs = HttpContext.Session.GetObject<List<TariffBase>>("SelectedTariffs");
            if (Profile == null || tariffs == null)
                return;

            foreach (var tariff in tariffs)
            {
                decimal cost = tariff.CalculateCost(Profile);
                Results.Add(new TariffCostResult
                {
                    Tariff = tariff,
                    Cost = cost
                });
            }

            // Отсортировать по стоимости 
            for (int i = 0; i < Results.Count - 1; i++)
            {
                for (int j = i + 1; j < Results.Count; j++)
                {
                    if (Results[j].Cost < Results[i].Cost)
                    {
                        var temp = Results[i];
                        Results[i] = Results[j];
                        Results[j] = temp;
                    }
                }
            }
        }
    }

    public class TariffCostResult
    {
        public TariffBase Tariff { get; set; }
        public decimal Cost { get; set; }
    }
}