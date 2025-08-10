using EnergyTariffAdvisor.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Reflection;
using EnergyTariffAdvisor.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Reflection;

namespace EnergyTariffAdvisor.Pages
{
    public class SurveyInputModel : PageModel
    {
        private readonly HouseholdSurveyCsvService _csvService;

        public SurveyInputModel(HouseholdSurveyCsvService csvService)
        {
            _csvService = csvService;
        }

        [BindProperty]
        public HouseholdSurveyTab Survey { get; set; } = new();

        [BindProperty]
        public int? LoadId { get; set; }

        public List<PropertyInfo> SurveyProperties { get; set; } = new();
        public Dictionary<string, string> QuestionLabels { get; set; } = new();

        public void OnGet()
        {
            SurveyProperties = typeof(HouseholdSurveyTab).GetProperties().ToList();
            QuestionLabels = HouseholdSurveyQuestionLabels.GetLabels();
        }

        public IActionResult OnPostLoad()
        {
            //SurveyProperties = new List<PropertyInfo>(typeof(HouseholdSurveyTab).GetProperties());
            SurveyProperties = typeof(HouseholdSurveyTab).GetProperties().ToList();
            QuestionLabels = HouseholdSurveyQuestionLabels.GetLabels();

            if (LoadId.HasValue)
            {
                var loaded = _csvService.LoadSurveyById("csv/survey_only.csv", ("household_" + LoadId.Value));
                if (loaded != null)
                    Survey = loaded;
            }
            return Page();
        }

        public IActionResult OnPostSave()
        {
            SurveyProperties = new List<PropertyInfo>(typeof(HouseholdSurveyTab).GetProperties());
            //_csvService.SaveSurvey("csv/survey_only.csv", Survey);
            return RedirectToPage();
        }
    }
}