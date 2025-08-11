using EnergyTariffAdvisor.Models;
using EnergyTariffAdvisor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection;
using System.Text.Json;

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

        [BindProperty]
        public HalfHourlyConsumptionProfile Profile { get; set; } = new HalfHourlyConsumptionProfile();

        public List<PropertyInfo> SurveyProperties { get; set; } = new();
        public Dictionary<string, string> QuestionLabels { get; set; } = new();

        public void OnGet()
        {
            SurveyProperties = typeof(HouseholdSurveyTab).GetProperties().ToList();
            QuestionLabels = HouseholdSurveyQuestionLabels.GetLabels();
        }

        public IActionResult OnPostLoad()
        {

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

        public async Task<IActionResult> OnPostSaveAsync()
        {
            SurveyProperties = typeof(HouseholdSurveyTab).GetProperties().ToList();

            // Plan:
            // - When collecting featureValues, replace any nulls with 0.
            // - Use Select to check for null and substitute 0 for int?/double? properties.
            // TODO: normalization!
            var normalizer = new Normalizer();


            var featureValues = SurveyProperties
                .Skip(1)
                .Select(p =>
                {
                    var value = p.GetValue(Survey);
                    if (value == null)
                    {
                        // Substitute 0 for nulls in int?/double? properties
                        if (p.PropertyType == typeof(int?) || p.PropertyType == typeof(double?))
                            return 0;

                    }
                    else if (value != null)
                    {
                        return normalizer.Normalize(p.Name, decimal.Parse(value.ToString()));
                    }

                    return value;
                })
                .ToArray();

            // Prepare payload for FastAPI in the required JSON format
            var payload = new
            {
                features = featureValues
            };

            using var httpClient = new HttpClient();
            var requestUri = "http://127.0.0.1:8001/predict";
            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await httpClient.PostAsync(requestUri, content);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                //"{\"predictions\":[[1404.1672418128376,1495.4941783913682,1304.753330084589,1217.3354190950515,1469.9204521660433,1696.9945952519558,1557.1783770903637,1345.9516457641575,1323.333971902545,1505.0994743501144,1808.3152563733174,2765.350969903119,2426.54737605311,2569.9193521628754,2730.734868443814,2972.948939117644,2625.419176483253,2307.945355975984,2219.688615945914,2283.6368947505516,2276.3361029631565,2276.472257634486,2254.7000260890104,2283.709096290781,2245.8292342456994,2121.5799384260513,2117.939033000241,1923.216212647025,1978.3696495746503,2135.1114951866566,2013.6115128381218,2347.6896090032383,2709.351091641141,3126.4258371728024,3631.001041504454,3882.030515821482,3698.953937103113,3704.621629646775,3733.7287652461014,3640.520109981833,3543.229321015075,3448.92061073696,3172.18568435092,2863.6655540469037,2493.735147961454,2075.536412384222,1847.213455648598,1628.5788694918458]]}"
                //var predictionResult = System.Text.Json.JsonSerializer.Deserialize<PredictionResponse>(responseBody);

                var jsonDoc = JsonDocument.Parse(responseBody);
                // ЅерЄм predictions[0] Ч потому что predictions Ч это массив массивов
                var predictionsArray = jsonDoc
                    .RootElement
                    .GetProperty("predictions")[0]   // первый массив
                    .EnumerateArray()
                    .Select(e => e.GetDecimal())
                    .ToArray();

                // “еперь predictionsArray Ч это decimal[48]
                var newConsumption = new List<decimal>();
                foreach (var val in predictionsArray)
                {
                    // turn this into kWh from W
                    //newConsumption.Add(val * 0.001m);
                    newConsumption.Add(val * 0.001m * (5m / 60m));
                }

                Profile.Consumption = newConsumption;

                HttpContext.Session.SetObject("UserProfile", Profile);
                return RedirectToPage("/ProfileEditor");

            }
            catch (Exception ex)
            {
                TempData["PredictionError"] = "Error occurred while requesting FastAPI: " + ex.Message;
            }

            return RedirectToPage();
        }

        public class PredictionResponse
        {
            public object? Predictions { get; set; }
        }
    }
}