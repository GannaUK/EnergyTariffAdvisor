using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnergyTariffAdvisor.Models;

namespace EnergyTariffAdvisor.Pages
{
    public class ProfileEditorModel : PageModel
    {
        [BindProperty]
        public HalfHourlyConsumptionProfile Profile { get; set; } = new HalfHourlyConsumptionProfile();

        public void OnGet()
        {
            var sessionProfile = HttpContext.Session.GetObject<HalfHourlyConsumptionProfile>("UserProfile");
            if (sessionProfile != null)
            {
                Profile = sessionProfile;
            }

            // Если профиль пуст, инициализируем тестовым значением (например, 0.5 на каждый интервал)
            if (Profile.Consumption == null || Profile.Consumption.Count != 48)
            {
                Profile.Consumption = new List<decimal>();
                for (int i = 0; i < 48; i++)
                {
                    Profile.Consumption.Add(0.5m);
                }
            }
        }

        public IActionResult OnPost(string action)
        {
            if (action == "reset")
            {
                for (int i = 0; i < Profile.Consumption.Count; i++)
                {
                    Profile.Consumption[i] = 0;
                }

                return Page();
            }
            else if (action == "random")
            {
                var rand = new Random();
                for (int i = 0; i < Profile.Consumption.Count; i++)
                {
                    Profile.Consumption[i] = Math.Round(((decimal)rand.NextDouble() * 0.9m) + 0.1m, 2);
                }

                return Page();
            }

            // Основное сохранение — если нажата кнопка "Use This Profile"
            if (!ModelState.IsValid)
            {
                return Page();
            }

            HttpContext.Session.SetObject("UserProfile", Profile);
            return RedirectToPage("/CompareTariffs");
        }
    }
}
