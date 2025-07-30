using TCBExchangeRate.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TCBExchangeRate.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Currency> Currencies => Set<Currency>();
        public DbSet<ExchangeRateSnapshot> ExchangeRateSnapshots => Set<ExchangeRateSnapshot>();
        public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}