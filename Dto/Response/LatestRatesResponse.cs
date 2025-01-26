namespace CurrencyConverter.Dto.Response
{
    public class LatestRatesResponse
    {
        public string Base { get; set; }
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; }
    }

}
