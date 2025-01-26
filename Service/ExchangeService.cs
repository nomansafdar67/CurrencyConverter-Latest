using CurrencyConverter.Dto.Request;
using CurrencyConverter.Dto.Response;
using CurrencyConverter.Interface;
using CurrencyConverter.Utilities;
using Microsoft.AspNetCore.Http.HttpResults;
using Polly.Wrap;

namespace CurrencyConverter.Service
{
    public class ExchangeService : IExchangeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ExchangeService> _logger;
        private readonly AsyncPolicyWrap _policy;

        public ExchangeService(IHttpClientFactory httpClientFactory, ILogger<ExchangeService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _policy = PolicyHelper.CreateCombinedPolicy(_logger);
        }

        public async Task<LatestRatesResponse> GetLatestRates(string baseCurrency)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FrankfurterApi");

                return await _policy.ExecuteAsync(async () =>
                {
                    var response = await client.GetAsync($"/latest?base={baseCurrency}");
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadFromJsonAsync<LatestRatesResponse>()
                           ?? throw new Exception("Failed to deserialize LatestRatesResponse.");
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetLatestRates for base currency {baseCurrency}.");
                throw new Exception("An error occurred while fetching the latest rates. Please try again later.");
            }
        }

        public async Task<ConversionResponse> ConvertCurrency(ConversionRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FrankfurterApi");
                request.SourceCurrency = request.SourceCurrency.ToUpper();
                request.TargetCurrency = request.TargetCurrency.ToUpper();
                return await _policy.ExecuteAsync(async () =>
                {
                    var response = await client.GetAsync($"/latest?base={request.SourceCurrency}");
                    response.EnsureSuccessStatusCode();
                    var rates = await response.Content.ReadFromJsonAsync<LatestRatesResponse>();

                //if (!(rates?.Rates?.ContainsKey(request.TargetCurrency) ?? false))
                //        return null;
                    var convertedAmount = request.Amount * rates.Rates[request.TargetCurrency];
                    return new ConversionResponse { ConvertedAmount = convertedAmount }
                           ?? throw new Exception("Failed to deserialize ConversionResponse.");
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in ConvertCurrency for request: {request}.");
                throw new Exception("An error occurred while converting currency. Please try again later.");
            }
        }

        public async Task<PaginatedResponse<HistoricalRatesResponse>> GetHistoricalRates(HistoricalRatesRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FrankfurterApi");

                return await _policy.ExecuteAsync(async () =>
                {
                    var url = $"/{request.StartDate}..{request.EndDate}?base={request.BaseCurrency}";
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var apiResponse = await response.Content.ReadFromJsonAsync<HistoricalRatesResponse>()
                          ?? throw new Exception("Failed to deserialize HistoricalRatesResponse.");

                    return PaginateRates(apiResponse, request.Page, request.PageSize);

                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetHistoricalRates for request: {request}.");
                throw new Exception("An error occurred while fetching historical rates. Please try again later.");
            }
        }

        public string[] ExcludedCurrencies => ["TRY", "PLN", "THB", "MXN"];

        #region Private

        private PaginatedResponse<HistoricalRatesResponse> PaginateRates(HistoricalRatesResponse rates, int page, int pageSize)
        {
            // Calculate total count of records
            var totalCount = rates.Rates.Count;

            // Validate page and pageSize
            if (page <= 0 || pageSize <= 0)
                throw new ArgumentException("Page and page size must be greater than zero.");

            // Calculate the subset of rates for the requested page
            var paginatedData = rates.Rates
                .Skip((page - 1) * pageSize)  // Skip records for previous pages
                .Take(pageSize)              // Take only the number of records for this page
                .ToDictionary(x => x.Key, x => x.Value);

            // Create a paginated response
            return new PaginatedResponse<HistoricalRatesResponse>
            {
                Data = new List<HistoricalRatesResponse>
        {
            new HistoricalRatesResponse
            {
                Base = rates.Base,
                Rates = paginatedData
            }
        },
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize
            };
        }
        #endregion
    }
}
