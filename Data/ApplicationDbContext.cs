using EnergyTariffAdvisor.Models;
using Microsoft.EntityFrameworkCore;

namespace EnergyTariffAdvisor.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

        public DbSet<FeedbackResponse> FeedbackResponses { get; set; }
    }
}
