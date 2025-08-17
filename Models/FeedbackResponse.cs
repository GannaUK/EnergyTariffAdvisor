using System;

namespace EnergyTariffAdvisor.Models
{
    public class FeedbackResponse
    {
        public int Id { get; set; }
        public bool? LiveInUK { get; set; }
        public bool? ProjectRelevant { get; set; }
        public bool? AIProfileAccurate { get; set; }
        public string Suggestions { get; set; }
        public bool? CalculatorBetter { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
