using Microsoft.EntityFrameworkCore;
using TCBExchangeRate.Domain.Entities;
using TCBExchangeRate.Infrastructure.Interfaces;
using TCBExchangeRate.Persistence;

namespace TCBExchangeRate.Infrastructure.Repositories
{
    public class ExchangeRateRepository : IExchangeRateRepository
    {
        private readonly AppDbContext _context;

        public ExchangeRateRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DateTime>> GetSnapshotTimesAsync(DateOnly date)
        {
            var snapshots = await _context.ExchangeRateSnapshots
                .Where(x => DateOnly.FromDateTime(x.SnapshotDateTime) == date)
                .Select(x => x.SnapshotDateTime)
                .ToListAsync();

            return snapshots;
        }

        public async Task AddSnapshotsAsync(IEnumerable<ExchangeRateSnapshot> snapshots)
        {
            await _context.ExchangeRateSnapshots.AddRangeAsync(snapshots);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
