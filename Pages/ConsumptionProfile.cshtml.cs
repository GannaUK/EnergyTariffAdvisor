using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace EnergyTariffAdvisor.Pages
{
    public class ConsumptionProfileModel : PageModel
    {
        // Binds the list of consumption values entered by the user (48 intervals)
        [BindProperty]
        public List<decimal> ConsumptionValues { get; set; } = new List<decimal>(new decimal[48]);

        // Handles GET requests - initializes ConsumptionValues list with zeros if null or incorrect size
        public void OnGet()
        {
            if (ConsumptionValues == null || ConsumptionValues.Count != 48)
            {
                ConsumptionValues = new List<decimal>(new decimal[48]);
            }
        }

        // Handles POST requests when the form is submitted
        public IActionResult OnPost()
        {
            // Validate the input: check if all 48 intervals are filled
            if (ConsumptionValues == null || ConsumptionValues.Count != 48)
            {
                ModelState.AddModelError(string.Empty, "Please fill in all 48 intervals.");
                return Page();
            }

            // TODO: Add tariff calculation and further processing here

            return Page();
        }
    }
}