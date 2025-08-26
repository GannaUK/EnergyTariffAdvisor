using System;
using System.Collections.Generic;

namespace EnergyTariffAdvisor.Models
{
    
    public class HalfHourlyConsumptionProfile
    {
        
        public List<decimal> Consumption { get; set; }

        public HalfHourlyConsumptionProfile()
        {
            
            Consumption = new List<decimal>(new decimal[48]);
        }

        
        public bool IsValid()
        {
            return Consumption != null && Consumption.Count == 48;
        }

        
        public static HalfHourlyConsumptionProfile GenerateExample()
        {
            var profile = new HalfHourlyConsumptionProfile();
            for (int i = 0; i < 48; i++)
            {                
                profile.Consumption[i] = (i >= 14 && i <= 18) || (i >= 38 && i <= 42) ? 0.5m : 0.2m;
            }
            return profile;
        }
    }
}