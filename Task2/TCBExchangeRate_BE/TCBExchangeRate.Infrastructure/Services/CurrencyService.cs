using TCBExchangeRate.Application.Interfaces;
using TCBExchangeRate.Application.Models.Responses;
using TCBExchangeRate.Infrastructure.Interfaces;

namespace TCBExchangeRate.Infrastructure.Services
{
	public class CurrencyService: ICurrencyService
    {
        private readonly ICurrencyRepository _currencyRepository;

        public CurrencyService(ICurrencyRepository currencyRepository)
        {
            _currencyRepository = currencyRepository;
        }

        public async Task<List<CurrencyResponse>> GetAllCurrenciesAsync()
        {
            var currencies = await _currencyRepository.GetAllCurrenciesAsync();

            return currencies.Select(c => new CurrencyResponse
            {
                Id = c.Id,
                Code = c.Code
            }).ToList();
        }
    }
}

