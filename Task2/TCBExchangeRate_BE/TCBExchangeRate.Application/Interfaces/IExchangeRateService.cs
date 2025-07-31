using TCBExchangeRate.Application.Models.Responses;

namespace TCBExchangeRate.Application.Interfaces
{
    public interface IExchangeRateService
    {
        Task<Result<int>> ImportExchangeRatesAsync(DateOnly date);
        Task<int> ImportExchangeRatesFromPastMonthAsync();
        Task<Result<List<ExchangeRateResponse>>> GetExchangeRateSnapshotsAsync(DateOnly date, string currencyCode);
    }
}
