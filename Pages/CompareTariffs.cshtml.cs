using EnergyTariffAdvisor.Models;
using EnergyTariffAdvisor.OctopusApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLitePCL;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EnergyTariffAdvisor.Pages
{
    public class CompareTariffsModel : PageModel
    {
        //private readonly OctopusTariffService _octopusService;
        private readonly IOctopusTariffProvider _octopusService;

        public CompareTariffsModel(IOctopusTariffProvider octopusService)
        {
            _octopusService = octopusService;
        }

        // Все доступные тарифы (Octopus, Ofgem, ручные)
        public List<TariffBase> AvailableTariffs { get; set; } = new();

        // Эти поля принимают данные из формы ручного ввода
        [BindProperty]
        public string ManualTariffName { get; set; }

        [BindProperty]
        public decimal ManualUnitRate { get; set; }

        [BindProperty]
        public decimal ManualStandingCharge { get; set; }

        [BindProperty]
        public List<int> SelectedTariffs { get; set; } = new();

        [BindProperty]
        public int Index { get; set; }

        public void OnGet()
        {
            // Подгружаем тарифы из Session или временного хранилища
            var storedTariffs = HttpContext.Session.GetObject<List<TariffBase>>("AvailableTariffs");
            if (storedTariffs != null)
                AvailableTariffs = storedTariffs;
        }

        public IActionResult OnPostAddManualTariff()
        {
            // Считываем текущий список
            var storedTariffs = HttpContext.Session.GetObject<List<TariffBase>>("AvailableTariffs") ?? new List<TariffBase>();

            // Создаем фиксированный тариф вручную
            var manualTariff = new FixedTariff(decimal.Parse(ManualUnitRate.ToString()),
                                                decimal.Parse(ManualStandingCharge.ToString()))
            {
                SupplierName = "My Current Supplier",
                TariffCode = "MANUAL",
                ProductName = "Manual Entry",
                Description = "User entered manually",
                TariffType = TariffType.Fixed
            };



            storedTariffs.Add(manualTariff);

            // Сохраняем обратно
            HttpContext.Session.SetObject("AvailableTariffs", storedTariffs);

            return RedirectToPage(); // Перезагружаем страницу
        }

        public IActionResult OnPostCalculate()
        {
            var storedTariffs = HttpContext.Session.GetObject<List<TariffBase>>("AvailableTariffs");
            if (storedTariffs == null)
                return RedirectToPage();

            var selected = new List<TariffBase>();
            foreach (int index in SelectedTariffs)
            {
                if (index >= 0 && index < storedTariffs.Count)
                    selected.Add(storedTariffs[index]);
            }

            // Сохраняем выбранные тарифы для дальнейшего анализа
            HttpContext.Session.SetObject("SelectedTariffs", selected);

            // Переход к странице результатов
            return RedirectToPage("ComparisonResults");
        }
        public async Task<IActionResult> OnPostLoadOctopusAsync()
        {
            //step 1 - Получаем все продукты
            var productsResponse = await _octopusService.GetProductsAsync();
            if (productsResponse == null || productsResponse.Results == null)
                return Page();

            var loadedTariffs = new List<TariffBase>();

            foreach (var product in productsResponse.Results)
            {
                // Пропускаем продукты, которые не являются импортными или бизнес-тарифами
                if (product.Direction != "IMPORT" || product.IsBusiness)
                    continue;
                // Пропускаем продукты, которые не имеют кода
                if (string.IsNullOrEmpty(product.Code))
                    continue;

                // Получаем ссылку self
                var selfLink = product.Links?.FirstOrDefault(l => l.Rel == "self")?.Href;
                if (string.IsNullOrEmpty(selfLink))
                    continue;

                // step 2 - Получаем детали продукта по ссылке self
                var productDetails = await _octopusService.GetProductDetailsByUrlAsync(selfLink);
                if (productDetails == null)
                    continue;


                // Метод для обработки словаря тарифов (чтобы не повторять код)
                async Task ProcessTariffDictionary(Dictionary<string, Dictionary<string, TariffDetailsDto>> tariffsDict)
                {
                    if (tariffsDict == null)
                        return;

                    foreach (var outerEntry in tariffsDict)
                    {
                        // Пропускаем тарифы, которые не относятся к региону "H"
                        if (outerEntry.Key != "_H")
                            continue;

                        foreach (var innerEntry in outerEntry.Value)
                        {
                            var tariffDetails = innerEntry.Value;
                            var tariffCode = tariffDetails.Code;

                            StandardUnitRatesResponse? ratesResponse = null;

                            try
                            {
                                ratesResponse = await _octopusService.GetStandardUnitRatesAsync(product.Code, tariffCode);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error loading standard unit rates for {product.Code}, {tariffCode}: {ex.Message}");
                                continue; // переходит к следующему тарифу
                            }

                            if (ratesResponse?.Results == null || ratesResponse.Results.Count == 0)
                                continue;

                            decimal dailyStandingCharge = 0m;
                            StandingChargesResponse? standingChargeResponse = null;

                            try
                            {
                                standingChargeResponse = await _octopusService.GetStandingChargesAsync(product.Code, tariffCode);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error loading standing charges for {tariffCode}: {ex.Message}");
                            }

                            if (ratesResponse?.Results == null)
                                continue;

                            TariffBase selectedTariff;
                            //если tariff_code содержит AGILE → agile
                            //если is_tracker == true → tracker
                            //если tariff_code содержит 2R → daynight
                            //иначе → static
                            if (tariffDetails.Code.Contains("AGILE"))
                            {
                                selectedTariff = new IntervalTariff
                                {
                                    TariffCode = tariffCode,

                                    ProductName = product.FullName ?? "",
                                    SupplierName = "Octopus Energy",
                                    Description = product.Description ?? "",
                                    TariffType = TariffType.Flexible,
                                    Href = selfLink ?? ""
                                };

                                var ratesSorted = new List<StandardUnitRateDto>(ratesResponse.Results);
                                ratesSorted.Sort((a, b) => a.ValidFrom.CompareTo(b.ValidFrom));

                                selectedTariff.UnitRatesPerInterval.Clear();

                                for (int i = 0; i < 48; i++)
                                {
                                    if (i < ratesSorted.Count)
                                        selectedTariff.UnitRatesPerInterval.Add(ratesSorted[i].ValueIncVat);
                                    else
                                        selectedTariff.UnitRatesPerInterval.Add(0m);
                                }
                            }
                            else if (product.IsTracker)
                            {
                                // TODO: implement TrackerTariff
                                selectedTariff = new IntervalTariff
                                {
                                    TariffCode = tariffCode,

                                    ProductName = product.FullName ?? "",
                                    SupplierName = "Octopus Energy",
                                    Description = product.Description ?? "",
                                    TariffType = TariffType.Tracker,
                                    Href = selfLink ?? ""
                                };
                            }
                            else if (tariffCode.Contains("GO"))
                            {
                                // TODO: implement DayNightTariff
                                var results = ratesResponse.Results;

                                decimal nightRate = 0m;
                                decimal dayRate = 0m;
                                List<decimal> uniqueRates = new List<decimal>();

                                foreach (var rate in results)
                                {
                                    uniqueRates.Add(rate.ValueIncVat);
                                }

                                nightRate = uniqueRates.Min();
                                dayRate = uniqueRates.Max();


                                selectedTariff = new DayNightTariff(
                                                    dayRate: dayRate,   
                                                    nightRate: nightRate
                                                  )
                                {
                                    TariffCode = tariffCode,

                                    ProductName = product.FullName ?? "",
                                    SupplierName = "Octopus Energy",
                                    Description = product.Description ?? "",
                                    TariffType = TariffType.DayNight,
                                    Href = selfLink ?? ""
                                };
                            }
                            else
                            {
                                // TODO: change to Fixed Tariff
                                selectedTariff = new IntervalTariff
                                {
                                    TariffCode = tariffCode,

                                    ProductName = product.FullName ?? "",
                                    SupplierName = "Octopus Energy",
                                    Description = product.Description ?? "",
                                    TariffType = TariffType.Fixed,
                                    Href = selfLink ?? ""
                                };
                            }

                            if (tariffDetails.StandardUnitRateIncVat > 0)
                            {
                                selectedTariff.UnitRate = tariffDetails.StandardUnitRateIncVat;
                            }
                            
                            selectedTariff.StandingChargeDaily = tariffDetails.StandingChargeIncVat;



                            loadedTariffs.Add(selectedTariff);
                        }
                    }
                }
                // Обрабатываем все три словаря тарифов
                await ProcessTariffDictionary(productDetails.SingleRegisterElectricityTariffs);
                await ProcessTariffDictionary(productDetails.DualRegisterElectricityTariffs);
                await ProcessTariffDictionary(productDetails.ThreeRateElectricityTariffs);
            }

            // Сохраняем загруженные тарифы в сессии
            HttpContext.Session.SetObject("AvailableTariffs", loadedTariffs);
            AvailableTariffs = loadedTariffs;

            return Page();
        }
        public IActionResult OnGetViewDetails(int index)
        {
            var storedTariffs = HttpContext.Session.GetObject<List<TariffBase>>("AvailableTariffs");
            if (storedTariffs == null || index < 0 || index >= storedTariffs.Count)
                return RedirectToPage();

            var selectedTariff = storedTariffs[index];
            HttpContext.Session.SetObject("TariffDetails", selectedTariff);

            return RedirectToPage("TariffDetails");
        }
    }
}