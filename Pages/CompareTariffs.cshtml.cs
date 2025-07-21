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

        // ��� ��������� ������ (Octopus, Ofgem, ������)
        public List<TariffBase> AvailableTariffs { get; set; } = new();

        // ��� ���� ��������� ������ �� ����� ������� �����
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
            // ���������� ������ �� Session ��� ���������� ���������
            var storedTariffs = HttpContext.Session.GetObject<List<TariffBase>>("AvailableTariffs");
            if (storedTariffs != null)
                AvailableTariffs = storedTariffs;
        }

        public IActionResult OnPostAddManualTariff()
        {
            // ��������� ������� ������
            var storedTariffs = HttpContext.Session.GetObject<List<TariffBase>>("AvailableTariffs") ?? new List<TariffBase>();

            // ������� ������������� ����� �������
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

            // ��������� �������
            HttpContext.Session.SetObject("AvailableTariffs", storedTariffs);

            return RedirectToPage(); // ������������� ��������
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

            // ��������� ��������� ������ ��� ����������� �������
            HttpContext.Session.SetObject("SelectedTariffs", selected);

            // ������� � �������� �����������
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
                // ���������� ��������, ������� �� �������� ���������� ��� ������-��������
                if (product.Direction != "IMPORT" || product.IsBusiness)
                    continue;
                // ���������� ��������, ������� �� ����� ����
                if (string.IsNullOrEmpty(product.Code))
                    continue;

                // �������� ������ self
                var selfLink = product.Links?.FirstOrDefault(l => l.Rel == "self")?.Href;
                if (string.IsNullOrEmpty(selfLink))
                    continue;

                var productDetails = await _octopusService.GetProductDetailsByUrlAsync(selfLink);
                if (productDetails == null)
                    continue;
                

                // ����� ��� ��������� ������� ������� (����� �� ��������� ���)
                async Task ProcessTariffDictionary(Dictionary<string, Dictionary<string, TariffDetailsDto>> tariffsDict)
                {
                    if (tariffsDict == null)
                        return;

                    foreach (var outerEntry in tariffsDict)
                    {
                        // ���������� ������, ������� �� ��������� � ������� "H"
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
                                Console.WriteLine($"������ ��� �������� ������ ������. product.Code: {product.Code}, tariffCode: {tariffCode}");
                                Console.WriteLine($"��������� �� ������: {ex.Message}");
                                continue; // ��������� � ���������� ������
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
                            //    intervalTariff.UnitRate = ratesResponse.Results[0].ValueIncVat; // ����� ������ ������ ��� �������
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
                // ������������ ��� ��� ������� �������
                await ProcessTariffDictionary(productDetails.SingleRegisterElectricityTariffs);
                await ProcessTariffDictionary(productDetails.DualRegisterElectricityTariffs);
                await ProcessTariffDictionary(productDetails.ThreeRateElectricityTariffs);
            }

            // ��������� ����������� ������ � ������
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