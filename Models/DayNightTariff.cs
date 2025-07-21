using System;
using System.Collections.Generic;

namespace EnergyTariffAdvisor.Models
{
    public class DayNightTariff : TariffBase
    {
        public decimal DayRate { get; set; }
        public decimal NightRate { get; set; }

        public DayNightTariff(decimal dayRate, decimal nightRate)
        {
            DayRate = dayRate;
            NightRate = nightRate;

            UnitRatesPerInterval = new List<decimal>();

            // Ночная ставка с 23:30 до 04:30 UTC = интервалы: 47, 0–8
            for (int i = 0; i < 48; i++)
            {
                if (i == 47 || (i >= 0 && i <= 8))
                    UnitRatesPerInterval.Add(NightRate);
                else
                    UnitRatesPerInterval.Add(DayRate);
            }

            TariffType = TariffType.DayNight;
        }

        public override string GetUnitRateDisplay()
        {
            return $"Day: {DayRate:0.###} p/kWh, Night: {NightRate:0.###} p/kWh";
        }
    }
}