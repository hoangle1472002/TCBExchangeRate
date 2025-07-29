using TCBExchangeRate.Domain.Entities;

namespace TCBExchangeRate.Infrastructure.Interfaces
{
    public interface ICurrencyRepository
    {
        Task<List<Currency>> GetByCodesAsync(IEnumerable<string> codes);
    }
}

