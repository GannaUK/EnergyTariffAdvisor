using EnergyTariffAdvisor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EnergyTariffAdvisor.Pages
{
    public class FeedbackModel : PageModel
    {
        private readonly Data.ApplicationDbContext _context;
        public FeedbackModel(Data.ApplicationDbContext context)
        {
            _context = context;
        }
        [BindProperty]
        public FeedbackResponse Response { get; set; } = new FeedbackResponse();

        public bool Submitted { get; set; } = false;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.FeedbackResponses.Add(Response);
            await _context.SaveChangesAsync();
            Submitted = true;
            return Page();
        }



        public void OnGet()
        {
        }
    }
}
