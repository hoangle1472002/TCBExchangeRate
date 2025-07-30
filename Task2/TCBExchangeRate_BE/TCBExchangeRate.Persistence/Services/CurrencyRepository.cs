using Microsoft.EntityFrameworkCore;
using TCBExchangeRate.Domain.Entities;
using TCBExchangeRate.Infrastructure.Interfaces;

namespace TCBExchangeRate.Persistence.Services
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly AppDbContext _context;

        public CurrencyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Currency>> GetByCodesAsync(IEnumerable<string> codes)
        {
            return await _context.Currencies
                .Where(c => codes.Contains(c.Code))
                .ToListAsync();
        }

        public async Task<List<Currency>> GetAllCurrenciesAsync()
        {
            return await _context.Currencies
                .OrderBy(c => c.Code)
                .ToListAsync();
        }
    }
}

