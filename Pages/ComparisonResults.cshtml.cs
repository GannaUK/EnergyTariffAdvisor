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

            foreach (var tariff in tariffs)
            {
                decimal cost = tariff.CalculateCost(Profile);
                Results.Add(new TariffCostResult
                {
                    Tariff = tariff,
                    Cost = cost
                });
            }

            // Сортируем по стоимости
            Results.Sort((a, b) => a.Cost.CompareTo(b.Cost));
        }

        // Добавляем обработчик для кнопки Details
        public IActionResult OnGetViewDetails(int index)
        {
            // TODO: Используем те же тарифы, что отображаются в результатах сравнения,здесь ошибка! Надо использовать те, что были выбраны на предыдущей странице!!!
            var storedTariffs = HttpContext.Session.GetObject<List<TariffBase>>("SelectedTariffs");
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