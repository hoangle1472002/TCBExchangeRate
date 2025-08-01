using Microsoft.Extensions.Logging;
using Quartz;
using TCBExchangeRate.Application.Interfaces;

namespace TCBExchangeRate.Infrastructure.Jobs
{
    public class ExchangeRateImportJob : IJob
    {
        private readonly IExchangeRateService _service;
        private readonly ILogger<ExchangeRateImportJob> _logger;

        public ExchangeRateImportJob(IExchangeRateService service, ILogger<ExchangeRateImportJob> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            _logger.LogInformation("Running scheduled import for {Date}", date);
            await _service.ImportExchangeRatesAsync(date);
            _logger.LogInformation("Ending scheduled import for {Date}", date);
        }
    }
}