using System;
using Microsoft.AspNetCore.Mvc;
using TCBExchangeRate.Application.Interfaces;

namespace TCBExchangeRate.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrenciesController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;

        public CurrenciesController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var currencies = await _currencyService.GetAllCurrenciesAsync();
            return Ok(currencies);
        }
    }
}
