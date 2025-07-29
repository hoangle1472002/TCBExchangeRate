namespace TCBExchangeRate.Application.Interfaces
{
    public interface IExchangeRateService
    {
        Task<int> ImportExchangeRatesAsync(DateOnly date);
    }
}

