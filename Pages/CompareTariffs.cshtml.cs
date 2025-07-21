using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnergyTariffAdvisor.Models;
using System.Collections.Generic;
using EnergyTariffAdvisor.OctopusApi;

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
            var manualTariff = new FixedTariff
            {
                //TariffName = ManualTariffName,
                SupplierName = "My Current Supplier",
                TariffCode = "MANUAL",
                ProductName = "Manual Entry",
                Description = "User entered manually",
                TariffType = TariffType.Fixed,
                StandingChargeDaily = ManualStandingCharge,
                UnitRate = ManualUnitRate
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
                                Console.WriteLine($"Ошибка при загрузке ставок тарифа. product.Code: {product.Code}, tariffCode: {tariffCode}");
                                Console.WriteLine($"Сообщение об ошибке: {ex.Message}");
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

                            var intervalTariff = new IntervalTariff
                            {
                                TariffCode = tariffCode,

                                //TariffName = tariffDetails.Code,
                                ProductName = product.FullName ?? "",
                                SupplierName = "Octopus Energy",
                                Description = product.Description ?? "",
                                TariffType = TariffType.Flexible
                            };

                            if (tariffDetails.StandardUnitRateIncVat>0)
                            {
                                intervalTariff.UnitRate = tariffDetails.StandardUnitRateIncVat;
                            }
                            //else
                            //{
                            //    intervalTariff.UnitRate = ratesResponse.Results[0].ValueIncVat; // Берем первую ставку как базовую
                            //}

                            if (standingChargeResponse?.Results != null)
                            {
                                foreach (var charge in standingChargeResponse.Results)
                                {
                                    dailyStandingCharge += charge.ValueIncVat;
                                }
                            }
                            intervalTariff.StandingChargeDaily = dailyStandingCharge;

                            var ratesSorted = new List<StandardUnitRateDto>(ratesResponse.Results);
                            ratesSorted.Sort((a, b) => a.ValidFrom.CompareTo(b.ValidFrom));

                            intervalTariff.UnitRatesPerInterval.Clear();

                            for (int i = 0; i < 48; i++)
                            {
                                if (i < ratesSorted.Count)
                                    intervalTariff.UnitRatesPerInterval.Add(ratesSorted[i].ValueIncVat);
                                else
                                    intervalTariff.UnitRatesPerInterval.Add(0m);
                            }

                            loadedTariffs.Add(intervalTariff);
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