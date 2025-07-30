using TCBExchangeRate.Application.Models.Responses;

namespace TCBExchangeRate.Application.Interfaces
{
    public interface IExchangeRateService
    {
        Task<int> ImportExchangeRatesAsync(DateOnly date);
        Task<List<ExchangeRateResponse>> GetExchangeRateSnapshotsAsync(DateOnly date, string currencyCode);
    }
}
