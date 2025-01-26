using CurrencyConverter.Dto.Request;
using CurrencyConverter.Interface;
using CurrencyConverter.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CurrencyConverter.Controllers
{
    [ApiController]
    [Route("api/exchange")]
    public class CurrencyExchangeController : ControllerBase
    {
        private readonly IExchangeService _currencyService;
        private readonly ILogger<CurrencyExchangeController> _logger;

        public CurrencyExchangeController(IExchangeService currencyService , ILogger<CurrencyExchangeController> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        [HttpGet("latestrates")]
        public async Task<IActionResult> GetLatestRates([FromQuery] string baseCurrency)
        {
            _logger.LogInformation("Hit getlatesrates");   
            var response = await _currencyService.GetLatestRates(baseCurrency);
            if (response == null) return BadRequest("Invalid base currency.");
            return Ok(response);
        }

        [HttpPost("convertcurrency")]
        public async Task<IActionResult> ConvertCurrency([FromBody] ConversionRequest request)
        {

            // Check if either the source or target currency is excluded
            if (IsCurrencyExcluded(request.SourceCurrency) ||
                IsCurrencyExcluded(request.TargetCurrency))
            {
                return BadRequest($"Currency conversion for TRY, PLN, THB, and MXN is not supported.");
            }

            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be greater than zero.");
            }

            var response = await _currencyService.ConvertCurrency(request);
            if (response == null) return BadRequest("Conversion not supported.");
            return Ok(response);
        }

        private bool IsCurrencyExcluded(string sourceCurrency)
        {
            return _currencyService.ExcludedCurrencies.Contains(sourceCurrency, StringComparer.OrdinalIgnoreCase);
        }

        [HttpGet("historicalrates")]
        public async Task<IActionResult> GetHistoricalRates([FromQuery] HistoricalRatesRequest request)
        {
            var StartDate = DateTime.Parse(request.StartDate);
            var EndDate = DateTime.Parse(request.EndDate);
            if(EndDate < StartDate)
            {
                return BadRequest("Start date cannot be greater then End date");
            }

            if(string.IsNullOrEmpty(request.BaseCurrency) || request.Page == 0 || request.PageSize == 0 || string.IsNullOrEmpty(request.EndDate) || string.IsNullOrEmpty(request.StartDate) )
            {
                return BadRequest("All Fields are mandatory");
            }
             var response = await _currencyService.GetHistoricalRates(request);
            return Ok(response);
        }
    }
}
