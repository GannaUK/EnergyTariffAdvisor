using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EnergyTariffAdvisor.Models;
using System.Collections.Generic;

namespace EnergyTariffAdvisor.Pages
{
    public class CompareTariffsModel : PageModel
    {
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
                TariffName = ManualTariffName,
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
    }
}