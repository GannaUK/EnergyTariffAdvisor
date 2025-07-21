using EnergyTariffAdvisor.Models;

namespace EnergyTariffAdvisor.Models
{
    // Абстрактный базовый класс для тарифов
    public abstract class TariffBase
    {
        public string TariffCode { get; set; }

        public string ProductName { get; set; }
        public string SupplierName { get; set; }
        public string Description { get; set; }
        public TariffType TariffType { get; set; }
        public decimal StandingChargeDaily { get; set; } = 0;
        public decimal UnitRate { get; set; }

        public decimal AdditionalFee { get; set; } = 0;
        public string Href { get; set; } = string.Empty;

        //пробую перенести код в общий класс
        public List<decimal> UnitRatesPerInterval { get; set; } = new List<decimal>();

      
        // Расчёт стоимости — общий для всех тарифов
        // calculate cost — common for all tariffs
        public decimal CalculateCost(HalfHourlyConsumptionProfile profile)
        {
            if (profile == null || profile.Consumption == null)
                throw new ArgumentException("Invalid consumption profile");

            if (profile.Consumption.Count != UnitRatesPerInterval.Count)
                throw new InvalidOperationException("Mismatch between consumption and tariff intervals");

            decimal totalCost = StandingChargeDaily + AdditionalFee;

            for (int i = 0; i < 48; i++)
            {
                totalCost += profile.Consumption[i] * UnitRatesPerInterval[i];
            }

            return totalCost;
        }

        // Отображение тарифов — индивидуально для каждого тарифа
        // Display of rates — individually for each tariff
        public abstract string GetUnitRateDisplay();
    }
}