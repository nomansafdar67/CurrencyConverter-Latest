using CurrencyConverter.Dto.Request;
using CurrencyConverter.Dto.Response;
using CurrencyConverter.Service;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using Xunit;

namespace CurrencyConverter.TestCase
{
    public class ExchangeServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ILogger<ExchangeService>> _loggerMock;
        private ExchangeService _currencyService;

        public ExchangeServiceTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _loggerMock = new Mock<ILogger<ExchangeService>>();
        }

        private void SetupHttpClientFactory(HttpResponseMessage responseMessage)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            // Mock the protected "SendAsync" method
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            // Create the HttpClient using the mocked HttpMessageHandler
            var client = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.frankfurter.app")
            };

            // Mock the IHttpClientFactory to return the mocked HttpClient
            _httpClientFactoryMock
                .Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(client);

            // Initialize the service with the mocked HttpClientFactory and Logger
            _currencyService = new ExchangeService(_httpClientFactoryMock.Object, _loggerMock.Object);
        }

        #region LatestRatesTests


        [Fact]
        public async Task GetLatestRates_ShouldReturnLatestRates_WhenRequestIsSuccessful()
        {
            // Arrange
            var baseCurrency = "USD";
            var expectedRates = new LatestRatesResponse
            {
                Base = "USD",
                Rates = new System.Collections.Generic.Dictionary<string, decimal>
                {
                    { "EUR", 0.85m },
                    { "GBP", 0.75m }
                }
            };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(expectedRates))
            };

            SetupHttpClientFactory(mockResponse);

            
            var result = await _currencyService.GetLatestRates(baseCurrency);

            
            Assert.NotNull(result);
            Assert.Equal(expectedRates.Base, result.Base);
            Assert.Equal(expectedRates.Rates["EUR"], result.Rates["EUR"]);
            Assert.Equal(expectedRates.Rates["GBP"], result.Rates["GBP"]);
        }

        [Fact]
        public async Task GetLatestRates_ShouldThrowException_WhenApiFails()
        {
            
            var baseCurrency = "USD";
            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            SetupHttpClientFactory(mockResponse);

            
            var exception = await Assert.ThrowsAsync<Exception>(() => _currencyService.GetLatestRates(baseCurrency));
            Assert.Equal("An error occurred while fetching the latest rates. Please try again later.", exception.Message);
        }

        [Fact]
        public async Task GetLatestRates_ShouldThrowException_WhenResponseCannotBeDeserialized()
        {
          
            var baseCurrency = "USD";
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"base\":\"USD\",\"rates\":}")
            };

            SetupHttpClientFactory(mockResponse);

            
            var exception = await Assert.ThrowsAsync<Exception>(() => _currencyService.GetLatestRates(baseCurrency));
            Assert.Equal("An error occurred while fetching the latest rates. Please try again later.", exception.Message);
        }

        #endregion

        #region CurrencyConverionTests

        [Fact]
        public async Task ConvertCurrency_ShouldReturnConvertedAmount_WhenRequestIsSuccessful()
        {
           
            var request = new ConversionRequest
            {
                SourceCurrency = "USD",
                TargetCurrency = "EUR",
                Amount = 100
            };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"base\":\"USD\",\"rates\":{\"EUR\":0.85}}")
            };

            SetupHttpClientFactory(mockResponse);

            
            var result = await _currencyService.ConvertCurrency(request);

            
            Assert.NotNull(result);
            Assert.Equal(85, result.ConvertedAmount);
        }

        [Fact]
        public async Task ConvertCurrency_ShouldReturnNull_WhenTargetCurrencyIsNotFoundInRates()
        {
            
            var request = new ConversionRequest
            {
                SourceCurrency = "USD",
                TargetCurrency = "GBP",
                Amount = 100
            };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"base\":\"USD\",\"rates\":{\"EUR\":0.85}}") 
            };

            SetupHttpClientFactory(mockResponse);

           
            var result = await _currencyService.ConvertCurrency(request);

            
            Assert.Null(result);
        }

        [Fact]
        public async Task ConvertCurrency_ShouldThrowException_WhenHttpRequestFails()
        {
            var request = new ConversionRequest
            {
                SourceCurrency = "USD",
                TargetCurrency = "EUR",
                Amount = 100
            };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            SetupHttpClientFactory(mockResponse);

            
            var exception = await Assert.ThrowsAsync<Exception>(() => _currencyService.ConvertCurrency(request));
            Assert.Equal("An error occurred while converting currency. Please try again later.", exception.Message);
        }


        #endregion

        #region GetHistoricalRates

        [Fact]
        public async Task GetHistoricalRates_ShouldReturnPaginatedResponse_WhenRequestIsSuccessful()
        {
            // Arrange
            var request = new HistoricalRatesRequest
            {
                BaseCurrency = "USD",
                StartDate = "2020-01-01",
                EndDate = "2020-01-31",
                Page = 1,
                PageSize = 10
            };

            var historicalRatesResponse = new HistoricalRatesResponse
            {
                Base = "USD",
                Rates = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, decimal>>
                {
                    { "2020-01-01", new System.Collections.Generic.Dictionary<string, decimal> { { "EUR", 0.85m } } },
                    { "2020-01-02", new System.Collections.Generic.Dictionary<string, decimal> { { "EUR", 0.86m } } }
                }
            };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(historicalRatesResponse))
            };

            SetupHttpClientFactory(mockResponse);

            var result = await _currencyService.GetHistoricalRates(request);

            
            Assert.NotNull(result);
            Assert.Equal(historicalRatesResponse.Base, result.Data.FirstOrDefault().Base);
        }

        [Fact]
        public async Task GetHistoricalRates_ShouldThrowException_WhenApiFails()
        {
            var request = new HistoricalRatesRequest
            {
                BaseCurrency = "USD",
                StartDate = "2020-01-01",
                EndDate = "2020-01-31",
                Page = 1,
                PageSize = 10
            };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            SetupHttpClientFactory(mockResponse);

            var exception = await Assert.ThrowsAsync<Exception>(() => _currencyService.GetHistoricalRates(request));
            Assert.Equal("An error occurred while fetching historical rates. Please try again later.", exception.Message);
        }

        [Fact]
        public async Task GetHistoricalRates_ShouldThrowException_WhenResponseCannotBeDeserialized()
        {
            // Arrange
            var request = new HistoricalRatesRequest
            {
                BaseCurrency = "USD",
                StartDate = "2020-01-01",
                EndDate = "2020-01-31",
                Page = 1,
                PageSize = 10
            };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"base\":\"USD\",\"rates\":}") // Invalid JSON for rates
            };

            SetupHttpClientFactory(mockResponse);

            var exception = await Assert.ThrowsAsync<Exception>(() => _currencyService.GetHistoricalRates(request));
            Assert.Equal("An error occurred while fetching historical rates. Please try again later.", exception.Message);
        }

        #endregion

    }
}
