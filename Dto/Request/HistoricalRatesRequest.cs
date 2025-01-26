namespace CurrencyConverter.Dto.Request
{
    public class HistoricalRatesRequest
    {
        public string BaseCurrency { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
