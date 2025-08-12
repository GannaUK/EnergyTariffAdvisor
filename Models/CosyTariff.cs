using System.Collections.Generic;
using System.Linq;

namespace EnergyTariffAdvisor.Models
{
    public class CosyTariff : TariffBase
    {
        public List<OctopusApi.StandardUnitRateDto> RatesList { get; set; } = new List<OctopusApi.StandardUnitRateDto>();
        public CosyTariff(List<OctopusApi.StandardUnitRateDto> RatesList)
        {
            // Сортируем rates по ValidFrom для последовательной обработки
            var ratesSorted = RatesList
                .OrderBy(r => r.ValidFrom)
                .ToList();

            // Определяем начало дня (вчера)
            DateTime dayStart = DateTime.Today.AddDays(-1); // 00:00:00 предыдущего дня
            
            
                decimal rateValue = 0m; // Значение по умолчанию, если не найдено
            // Заполняем 48 интервалов (каждые 30 минут)
            for (int i = 0; i < 48; i++)
            {
                // Начало текущего 30-минутного слота
                DateTime slotStart = dayStart.AddMinutes(i * 30);
                              

                foreach (var rate in ratesSorted)
                {
                    // Проверяем, попадает ли slotStart в интервал [ValidFrom, ValidTo)
                    // Предполагаем, что ValidTo - эксклюзивное (не включая саму ValidTo)
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
