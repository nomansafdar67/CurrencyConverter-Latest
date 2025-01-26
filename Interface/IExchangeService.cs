
using CurrencyConverter.Dto.Request;
using CurrencyConverter.Dto.Response;

namespace CurrencyConverter.Interface
{
    public interface IExchangeService
    {
        Task<LatestRatesResponse> GetLatestRates(string baseCurrency);
        Task<ConversionResponse> ConvertCurrency(ConversionRequest request);
        Task<PaginatedResponse<HistoricalRatesResponse>> GetHistoricalRates(HistoricalRatesRequest request);
        string[] ExcludedCurrencies { get; }
    }

}
