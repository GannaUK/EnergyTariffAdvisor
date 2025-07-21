using System;

namespace EnergyTariffAdvisor.Models
{
    public class FixedTariff : TariffBase
    {
        
        public override decimal CalculateCost(HalfHourlyConsumptionProfile profile)
        {
            decimal totalConsumption = 0;
            for (int i = 0; i < profile.Consumption.Count; i++)
            {
                totalConsumption += profile.Consumption[i];
            }

            decimal energyCost = totalConsumption * UnitRate;

            // Стоимость за подключение (стоимость за каждый день, если профайл — на сутки)
            //decimal standingCharge = StandingChargeDaily;

            return energyCost + StandingChargeDaily;
        }

        public override string GetUnitRateDisplay()
        {
            return $"{UnitRate.ToString("0.###")} p/kWh";
        }
    }
}
