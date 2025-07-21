using System.Collections.Generic;
using System.Linq;

namespace EnergyTariffAdvisor.Models
{
    public class IntervalTariff : TariffBase
    {
        
        public override string GetUnitRateDisplay()
        {
            if (UnitRate > 0)
                return $"{UnitRate.ToString("0.###")} p/kWh";

            decimal average = UnitRatesPerInterval.Sum() / UnitRatesPerInterval.Count;
            return $"{average.ToString("0.###")} p/kWh (avg)";
        }
    }
}
