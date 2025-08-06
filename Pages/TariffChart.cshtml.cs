using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using EnergyTariffAdvisor.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;

namespace EnergyTariffAdvisor.Pages
{
    public class TariffChartModel : PageModel
    {
        public TariffBase? Tariff { get; set; }
        public HalfHourlyConsumptionProfile? Profile { get; set; }

        public List<IntervalData> IntervalChartData { get; set; } = new();

        public decimal totalUsage => Profile?.Consumption != null ? Math.Round(Profile.Consumption.Sum(), 2) : 0;
        public decimal totalCost => Tariff != null && Profile?.Consumption != null ? Math.Round(Tariff.CalculateCost(Profile), 2) : 0;
        public decimal grandTotalCost => totalCost + Tariff?.StandingChargeDaily / 100 ?? 0; // Convert pence to pounds

        public IActionResult OnGet()
        {
            Tariff = HttpContext.Session.GetObject<TariffBase>("TariffDetails");
            Profile = HttpContext.Session.GetObject<HalfHourlyConsumptionProfile>("UserProfile");

            if (Tariff == null || Profile == null)
            {
                TempData["Error"] = "Missing tariff or consumption profile.";
                return RedirectToPage("/CompareTariffs");
            }

            DateTime baseTime = DateTime.Today;

            for (int i = 0; i < 48; i++)
            {
                decimal pricePerKwh = Tariff.GetUnitRateForInterval(i);
                decimal consumption = Profile.Consumption[i];
                decimal cost = pricePerKwh * consumption;

                IntervalChartData.Add(new IntervalData
                {
                    Index = i,
                    Time = baseTime.AddMinutes(30 * i).ToString("HH:mm"),
                    UnitRate = pricePerKwh,
                    Consumption = consumption,
                    Cost = cost
                });
            }

            return Page();
        }

        public class IntervalData
        {
            public int Index { get; set; }
            public string Time { get; set; } = string.Empty;
            public decimal UnitRate { get; set; }
            public decimal Consumption { get; set; }
            public decimal Cost { get; set; }
        }
        public string IntervalToTime(int interval)
        {
            // »нтервалы по 30 минут, 0 Ч это 00:00, 1 Ч 00:30 и т.д. - intervals of 30 minutes, 0 is 00:00, 1 is 00:30, etc.
            int totalMinutes = interval * 30;
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;
            return $"{hours:D2}:{minutes:D2}";
        }
    }
}
