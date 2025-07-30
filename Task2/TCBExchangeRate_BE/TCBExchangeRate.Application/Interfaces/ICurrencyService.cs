using TCBExchangeRate.Application.Models.Responses;

namespace TCBExchangeRate.Application.Interfaces
{
	public interface ICurrencyService
	{
        Task<Result<List<CurrencyResponse>>> GetAllCurrenciesAsync();
    }
}
