namespace CurrencyConverter.Dto.Request
{
    public class ConversionRequest
    {
        public decimal Amount { get; set; }
        public string SourceCurrency { get; set; }
        public string TargetCurrency { get; set; }
    }

}
