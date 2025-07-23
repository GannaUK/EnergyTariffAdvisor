using Microsoft.AspNetCore.Mvc;
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
            var tariffs = HttpContext.Session.GetObject<List<TariffBase>>("SelectedTariffs");
            if (Profile == null || tariffs == null)
                return;

            Results.Clear();

            var comparisonTariffs = new List<TariffBase>();

            foreach (var tariff in tariffs)
            {
                decimal cost = tariff.CalculateCost(Profile);
                Results.Add(new TariffCostResult
                {
                    Tariff = tariff,
                    Cost = cost
                });
                comparisonTariffs.Add(tariff);
            }

            // Сортируем по стоимости - sorted results and comparison tariffs by cost
            Results.Sort((a, b) => a.Cost.CompareTo(b.Cost));
            comparisonTariffs.Sort((a, b) => a.CalculateCost(Profile).CompareTo(b.CalculateCost(Profile)));

            HttpContext.Session.SetObject("ComparisonTariffs", comparisonTariffs);
        }

        // Добавляем обработчик для кнопки Details - add handler for Details button
        public IActionResult OnGetViewDetails(int index)
        {
            
            var storedTariffs = HttpContext.Session.GetObject<List<TariffBase>>("ComparisonTariffs");
            if (storedTariffs == null || index < 0 || index >= storedTariffs.Count)
                return RedirectToPage();

            var selectedTariff = storedTariffs[index];
            HttpContext.Session.SetObject("TariffDetails", selectedTariff);

            return RedirectToPage("TariffDetails");
        }
    }

    public class TariffCostResult
    {
        public TariffBase Tariff { get; set; }
        public decimal Cost { get; set; }
    }
}