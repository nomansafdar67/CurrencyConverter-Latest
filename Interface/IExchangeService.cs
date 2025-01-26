
using CurrencyConverter.Dto.Request;
using CurrencyConverter.Dto.Response;

namespace CurrencyConverter.Interface
{
    public interface IExchangeService
    {
        /// <summary>
        /// Retrieves the latest exchange rates for a given base currency.
        /// </summary>
        /// <param name="baseCurrency">The base currency code (e.g., EUR).</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the latest exchange rates.</returns>
        Task<LatestRatesResponse> GetLatestRates(string baseCurrency);

        /// <summary>
        /// Converts an amount from one currency to another.
        /// </summary>
        /// <param name="request">The conversion request containing the amount, source currency, and target currency.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the converted amount or null if the conversion is not allowed.</returns>
        Task<ConversionResponse> ConvertCurrency(ConversionRequest request);

        /// <summary>
        /// Retrieves the historical exchange rates for a given period using pagination.
        /// </summary>
        /// <param name="request">The historical rates request containing the date range, base currency, page number, and page size.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains paginated historical rates.</returns>
        Task<PaginatedResponse<HistoricalRatesResponse>> GetHistoricalRates(HistoricalRatesRequest request);

        string[] ExcludedCurrencies { get; }
    }

}
