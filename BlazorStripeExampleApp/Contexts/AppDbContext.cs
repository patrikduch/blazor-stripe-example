using BlazorStripeExample.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorStripeExample.Contexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Payment> Payments => Set<Payment>();
    }
}
