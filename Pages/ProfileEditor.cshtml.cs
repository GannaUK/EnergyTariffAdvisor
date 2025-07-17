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

            // ≈сли профиль пуст, инициализируем тестовым значением (например, 0.5 на каждый интервал)
            if (Profile.Consumption == null || Profile.Consumption.Count != 48)
            {
                Profile.Consumption = new List<decimal>();
                for (int i = 0; i < 48; i++)
                {
                    Profile.Consumption.Add(0.5m);
                }
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // «десь можно временно сохранить профиль, например, в TempData или сессии
            //TempData["Profile"] = System.Text.Json.JsonSerializer.Serialize(Profile);
            HttpContext.Session.SetObject("UserProfile", Profile);

            return RedirectToPage("/SelectTariffs");
        }
    }
}
