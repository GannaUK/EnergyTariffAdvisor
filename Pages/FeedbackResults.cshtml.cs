using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EnergyTariffAdvisor.Data;
using EnergyTariffAdvisor.Models; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnergyTariffAdvisor.Pages
{
    public class FeedbackResultsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public FeedbackResultsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<FeedbackResponse> FeedbackResponses { get; set; }

        public async Task OnGetAsync()
        {
            FeedbackResponses = await _context.FeedbackResponses.ToListAsync();
        }
    }
}
