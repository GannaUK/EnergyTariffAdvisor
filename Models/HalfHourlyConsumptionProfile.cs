using System;
using System.Collections.Generic;

namespace EnergyTariffAdvisor.Models
{
    // Профиль потребления за сутки: 48 получасовых интервалов
    public class HalfHourlyConsumptionProfile
    {
        // Потребление в кВт·ч для каждого 30-минутного интервала
        public List<decimal> Consumption { get; set; }

        public HalfHourlyConsumptionProfile()
        {
            // Инициализируем 48 значений нулями
            Consumption = new List<decimal>(new decimal[48]);
        }

        // Метод для проверки корректности профиля
        public bool IsValid()
        {
            return Consumption != null && Consumption.Count == 48;
        }

        // Создание образца профиля потребления для примера
        public static HalfHourlyConsumptionProfile GenerateExample()
        {
            var profile = new HalfHourlyConsumptionProfile();
            for (int i = 0; i < 48; i++)
            {
                // Пример: большее потребление утром и вечером
                profile.Consumption[i] = (i >= 14 && i <= 18) || (i >= 38 && i <= 42) ? 0.5m : 0.2m;
            }
            return profile;
        }
    }
}