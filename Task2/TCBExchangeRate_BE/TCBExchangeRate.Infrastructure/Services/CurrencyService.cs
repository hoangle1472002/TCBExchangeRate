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

        public async Task<Result<List<CurrencyResponse>>> GetAllCurrenciesAsync()
        {
            try
            {
                var currencies = await _currencyRepository.GetAllCurrenciesAsync();

                var response = currencies.Select(c => new CurrencyResponse
                {
                    Id = c.Id,
                    Code = c.Code
                }).ToList();

                return Result<List<CurrencyResponse>>.Ok(response);
            }
            catch (Exception ex)
            {
                return Result<List<CurrencyResponse>>.Fail("Failed to get currencies", new() { ex.Message });
            }
        }
    }
}

