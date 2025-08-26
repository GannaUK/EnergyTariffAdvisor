using System.Collections.Generic;
using System.Linq;

namespace EnergyTariffAdvisor.Models
{
    public class CosyTariff : TariffBase
    {
        public List<OctopusApi.StandardUnitRateDto> RatesList { get; set; } = new List<OctopusApi.StandardUnitRateDto>();
        public CosyTariff(List<OctopusApi.StandardUnitRateDto> RatesList)
        {
            // Sort rates by ValidFrom for sequential processing
            var ratesSorted = RatesList
                .OrderBy(r => r.ValidFrom)
                .ToList();

            // Determine the start of the day (yesterday)
            DateTime dayStart = DateTime.Today.AddDays(-1); // 00:00:00 of the previous day


            decimal rateValue = 0m; // Default value if not found
                                    // Fill 48 intervals (every 30 minutes)
            for (int i = 0; i < 48; i++)
            {
                // Start of the current 30-minute slot
                DateTime slotStart = dayStart.AddMinutes(i * 30);


                foreach (var rate in ratesSorted)
                {
                    // Check if slotStart falls within the interval [ValidFrom, ValidTo)
                    // Assume ValidTo is exclusive (not including ValidTo itself)
                    if (slotStart >= rate.ValidFrom && slotStart < rate.ValidTo)
                    {
                        rateValue = rate.ValueIncVat;
                        break;
                    }
                }

                UnitRatesPerInterval.Add(rateValue);
            }


            TariffType = TariffType.Cosy;
        }
        public override string GetUnitRateDisplay()
        {
            if (UnitRate > 0)
                return $"{UnitRate.ToString("0.###")} p/kWh";

            decimal average = UnitRatesPerInterval.Sum() / UnitRatesPerInterval.Count;
            return $"{average.ToString("0.###")} p/kWh (avg)";
        }
    }
}
