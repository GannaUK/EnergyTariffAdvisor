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
        //пробую перенести код в общий класс
        public List<decimal> UnitRatesPerInterval { get; set; } = new List<decimal>();

        // Абстрактный метод для расчёта стоимости по профилю потребления
        public abstract decimal CalculateCost(HalfHourlyConsumptionProfile profile);
        public abstract string GetUnitRateDisplay();
    }
}