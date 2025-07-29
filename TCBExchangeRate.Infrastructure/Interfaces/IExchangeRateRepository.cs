using TCBExchangeRate.Domain.Entities;

namespace TCBExchangeRate.Infrastructure.Interfaces
{
    public interface IExchangeRateRepository
    {
        Task<List<DateTime>> GetSnapshotTimesAsync(DateOnly date);
        Task AddSnapshotsAsync(IEnumerable<ExchangeRateSnapshot> snapshots);
        Task SaveChangesAsync();
    }
}

