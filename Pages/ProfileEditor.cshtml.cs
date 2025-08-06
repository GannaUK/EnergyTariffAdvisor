using EnergyTariffAdvisor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

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

            // Если профиль пуст, инициализируем тестовым значением (например, 1 на каждый интервал)
            if (Profile.Consumption == null || Profile.Consumption.Count != 48)
            {
                Profile.Consumption = new List<decimal>();
                for (int i = 0; i < 48; i++)
                {
                    Profile.Consumption.Add(1m);
                }
            }
        }

        public IActionResult OnPost(string action, IFormFile? csvFile)
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
            else if (action == "upload")
            {
                if (csvFile == null || csvFile.Length == 0)
                {
                    TempData["ProfileWarning"] = "Please select a CSV file.";
                    return Page();
                }

                try
                {
                    using var reader = new StreamReader(csvFile.OpenReadStream());
                    var content = reader.ReadToEnd();

                    // Разделение на числа, поддержка как запятой, так и новой строки
                    var values = content
                        .Trim()
                        .Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    if (values.Length != 48)
                    {
                        TempData["ProfileWarning"] = "CSV file must contain exactly 48 values.";
                        return Page();
                    }

                    var newConsumption = new List<decimal>();
                    foreach (var val in values)
                    {
                        if (!decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                        {
                            TempData["ProfileWarning"] = $"Invalid number in CSV: '{val}'.";
                            return Page();
                        }
                        // Преобразуем показания счетчика в кВт·ч, предполагая, что в CSV указано в Вт
                        // turn this into kWh from W
                        //newConsumption.Add(d * 0.001m);
                        newConsumption.Add(d * 0.001m * (5m / 60m));
                    }

                    Profile.Consumption = newConsumption;
                }
                catch (Exception ex)
                {
                    TempData["ProfileWarning"] = "Error reading CSV file: " + ex.Message;
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
