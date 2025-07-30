using Microsoft.AspNetCore.Mvc;
using TCBExchangeRate.Application.Interfaces;

namespace TCBExchangeRate.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeRateController: Controller
	{
        private readonly IExchangeRateService _exchangeRateService;

        public ExchangeRateController(IExchangeRateService exchangeRateService)
        {
            _exchangeRateService = exchangeRateService;
        }

        [HttpPost("fetch")]
        public async Task<IActionResult> Fetch([FromQuery] DateOnly date)
        {
            var count = await _exchangeRateService.ImportExchangeRatesAsync(date);
            return Ok(new { Saved = count });
        }

        [HttpGet("snapshots")]
        public async Task<IActionResult> GetExchangeRateSnapshots([FromQuery] DateOnly date, [FromQuery] string currencyCode)
        {
            var result = await _exchangeRateService.GetExchangeRateSnapshotsAsync(date, currencyCode);
            return Ok(result);
        }
    }
}

