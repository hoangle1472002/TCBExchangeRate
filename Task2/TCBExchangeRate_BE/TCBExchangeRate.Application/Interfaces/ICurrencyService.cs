using TCBExchangeRate.Application.Models.Responses;

namespace TCBExchangeRate.Application.Interfaces
{
	public interface ICurrencyService
	{
        Task<List<CurrencyResponse>> GetAllCurrenciesAsync();
    }
}

