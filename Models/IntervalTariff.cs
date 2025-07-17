using System.Collections.Generic;

namespace EnergyTariffAdvisor.Models
{
    public class IntervalTariff : TariffBase
    {
        // Список тарифных ставок по интервалам (48 значений за сутки)
        public List<decimal> UnitRatesPerInterval { get; set; } = new List<decimal>();

        // Переопределяем метод расчёта стоимости по профилю
        public override decimal CalculateCost(HalfHourlyConsumptionProfile profile)
        {
            decimal totalCost = 0;
            int count = profile.Consumption.Count;

            for (int i = 0; i < count; i++)
            {
                decimal consumption = profile.Consumption[i];
                decimal rate = 0;

                if (i < UnitRatesPerInterval.Count)
                {
                    rate = UnitRatesPerInterval[i];
                }

                totalCost += consumption * rate;
            }

            // Добавляем абонплату за день
            totalCost += StandingChargeDaily;

            return totalCost;
        }
    }
}
