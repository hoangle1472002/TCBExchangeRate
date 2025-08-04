using Microsoft.AspNetCore.Mvc;
using TCBProxyApp.Services;

namespace TCBProxyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeRateController : ControllerBase
    {
        private readonly ExchangeRateService _service;

        public ExchangeRateController(ExchangeRateService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string currencyCode, [FromQuery] string date)
        {
            if (string.IsNullOrWhiteSpace(currencyCode) || string.IsNullOrWhiteSpace(date))
                return BadRequest("currencyCode and date (yyyy-MM-dd) are required.");

            var result = await _service.GetExchangeRateByCurrencyAndDateAsync(currencyCode, date);

            if (result.Count == 0)
                return NoContent();

            return Ok(result.Select(x => new
            {
                x.Label,
                x.AskRate,
                x.AskRateTM,
                x.BidRateCK,
                x.BidRateTM,
                x.InputDate
            }));
        }
    }
}

