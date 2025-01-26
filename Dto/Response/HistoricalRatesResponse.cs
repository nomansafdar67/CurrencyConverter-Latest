namespace CurrencyConverter.Dto.Response
{
    public class HistoricalRatesResponse
    {
        public string Base { get; set; }
        public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; }
    }
}
