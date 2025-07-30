using Microsoft.EntityFrameworkCore;
using TCBExchangeRate.Domain.Entities;

namespace TCBExchangeRate.Persistence.Data
{
    public class CurrencySeeder
    {
        private readonly AppDbContext _context;

        private static readonly List<string> _currencyCodes = new()
    {
        "AUD", "CAD", "CHF", "CNY", "EUR", "GBP", "HKD", "JPY",
        "KRW", "NZD", "SGD", "THB", "USD (1,2)", "USD (5,10,20)", "USD (50,100)"
    };

        public CurrencySeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Currencies.AnyAsync()) return;

            foreach (var code in _currencyCodes)
            {
                _context.Currencies.Add(new Currency { Code = code, Name = code });
            }

            await _context.SaveChangesAsync();
        }
    }
}

