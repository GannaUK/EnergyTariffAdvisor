using System;

namespace EnergyTariffAdvisor.Models
{
    public class FixedTariff : TariffBase
    {
        public FixedTariff(decimal unitRate, decimal standingChargeDaily = 0, decimal additionalFee = 0)
        {
            UnitRate = unitRate;
            StandingChargeDaily = standingChargeDaily;
            AdditionalFee = additionalFee;

            // Fill 48 intervals with a single rate
            UnitRatesPerInterval = Enumerable.Repeat(unitRate, 48).ToList();

            TariffType = TariffType.Fixed; 
        }


        public override string GetUnitRateDisplay()
        {
            return $"{UnitRate.ToString("0.###")} p/kWh";
        }
    }
}
