using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using PurchaseTransactions.Exceptions;
using PurchaseTransactions.Services;
using System.Net;
using System.Text;

namespace PurchaseTransactions.Tests
{
    public class ExchangeRateServiceTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly IConfiguration _configuration;

        public ExchangeRateServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"FiscalApi:BaseUrl", "https://api.fiscaldata.treasury.gov/services/api/fiscal_service/v1/accounting/od/rates_of_exchange"}
                })
                .Build();
        }

        private ExchangeRateService CreateService()
        {
            var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            return new ExchangeRateService(_mockHttpClientFactory.Object, _configuration);
        }

        [Fact]
        public async Task GetRateForDateAsync_Throws_CurrencyCodeRequiredException_When_Currency_Is_Null()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<CurrencyCodeRequiredException>(() =>
                service.GetRateForDateAsync(null!, DateTime.UtcNow));
        }

        [Fact]
        public async Task GetRateForDateAsync_Throws_CurrencyCodeRequiredException_When_Currency_Is_Empty()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<CurrencyCodeRequiredException>(() =>
                service.GetRateForDateAsync("", DateTime.UtcNow));
        }

        [Fact]
        public async Task GetRateForDateAsync_Throws_CurrencyCodeRequiredException_When_Currency_Is_Whitespace()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<CurrencyCodeRequiredException>(() =>
                service.GetRateForDateAsync("   ", DateTime.UtcNow));
        }

        [Fact]
        public async Task GetRateForDateAsync_Throws_TreasuryApiException_When_Http_Error()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            var service = CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TreasuryApiException>(() =>
                service.GetRateForDateAsync("BRL", DateTime.UtcNow));

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }

        [Fact]
        public async Task GetRateForDateAsync_Throws_TreasuryApiException_When_Http_InternalServerError()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            var service = CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TreasuryApiException>(() =>
                service.GetRateForDateAsync("BRL", DateTime.UtcNow));

            Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
        }

        [Fact]
        public async Task GetRateForDateAsync_Throws_NoRatesFoundException_When_Data_Empty()
        {
            // Arrange
            var responseJson = @"{
                ""data"": []
            }";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                });

            var service = CreateService();
            var testDate = DateTime.UtcNow;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NoRatesFoundException>(() =>
                service.GetRateForDateAsync("BRL", testDate));

            Assert.Equal("BRL", exception.Currency);
            Assert.Equal(testDate, exception.TransactionDate);
        }

        [Fact]
        public async Task GetRateForDateAsync_Throws_FieldNotFoundException_When_RecordDate_Missing()
        {
            // Arrange
            var responseJson = @"{
                ""data"": [
                    {
                        ""exchange_rate"": ""5.50"",
                        ""currency"": ""Real""
                    }
                ]
            }";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                });

            var service = CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<FieldNotFoundException>(() =>
                service.GetRateForDateAsync("BRL", DateTime.UtcNow));

            Assert.Equal("record_date", exception.FieldName);
        }

        [Fact]
        public async Task GetRateForDateAsync_Throws_FieldNotFoundException_When_ExchangeRate_Missing()
        {
            // Arrange
            var responseJson = @"{
                ""data"": [
                    {
                        ""record_date"": ""2024-01-15"",
                        ""currency"": ""Real""
                    }
                ]
            }";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                });

            var service = CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<FieldNotFoundException>(() =>
                service.GetRateForDateAsync("BRL", DateTime.UtcNow));

            Assert.Equal("exchange_rate", exception.FieldName);
        }

        [Fact]
        public async Task GetRateForDateAsync_Throws_RateOutdatedException_When_Rate_Too_Old()
        {
            // Arrange
            var responseJson = @"{
                ""data"": [
                    {
                        ""record_date"": ""2024-01-15"",
                        ""exchange_rate"": ""5.50"",
                        ""currency"": ""Real""
                    }
                ]
            }";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                });

            var service = CreateService();
            var transactionDate = new DateTime(2024, 7, 30); // + 6 months

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RateOutdatedException>(() =>
                service.GetRateForDateAsync("BRL", transactionDate));

            Assert.Equal("BRL", exception.Currency);
        }

        [Fact]
        public async Task GetRateForDateAsync_Returns_Rate_When_Within_6_Months()
        {
            // Arrange
            var responseJson = @"{
                ""data"": [
                    {
                        ""record_date"": ""2024-06-15"",
                        ""exchange_rate"": ""5.50"",
                        ""currency"": ""Real""
                    }
                ]
            }";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                });

            var service = CreateService();
            var transactionDate = new DateTime(2024, 7, 15); // 1 mês depois

            // Act
            var (rate, rateDate) = await service.GetRateForDateAsync("BRL", transactionDate);

            // Assert
            Assert.Equal(5.50m, rate);
            Assert.Equal(new DateTime(2024, 6, 15), rateDate);
        }

        [Fact]
        public async Task GetRateForDateAsync_Returns_Rate_When_Exactly_6_Months()
        {
            // Arrange
            var responseJson = @"{
                ""data"": [
                    {
                        ""record_date"": ""2024-01-15"",
                        ""exchange_rate"": ""5.50"",
                        ""currency"": ""Real""
                    }
                ]
            }";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                });

            var service = CreateService();
            var transactionDate = new DateTime(2024, 7, 15); // Exatamente 6 meses depois

            // Act
            var (rate, rateDate) = await service.GetRateForDateAsync("BRL", transactionDate);

            // Assert
            Assert.Equal(5.50m, rate);
            Assert.Equal(new DateTime(2024, 1, 15), rateDate);
        }

        [Fact]
        public async Task GetRateForDateAsync_Uses_First_Item_From_Response()
        {
            // Arrange
            var responseJson = @"{
                ""data"": [
                    {
                        ""record_date"": ""2024-06-15"",
                        ""exchange_rate"": ""5.50"",
                        ""currency"": ""Real""
                    },
                    {
                        ""record_date"": ""2024-05-15"",
                        ""exchange_rate"": ""5.40"",
                        ""currency"": ""Real""
                    }
                ]
            }";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                });

            var service = CreateService();
            var transactionDate = new DateTime(2024, 7, 15);

            // Act
            var (rate, rateDate) = await service.GetRateForDateAsync("BRL", transactionDate);

            // Assert
            Assert.Equal(5.50m, rate); // Deve usar o primeiro item (mais recente)
            Assert.Equal(new DateTime(2024, 6, 15), rateDate);
        }

        [Fact]
        public async Task GetRateForDateAsync_Parses_Decimal_Correctly()
        {
            // Arrange
            var responseJson = @"{
                ""data"": [
                    {
                        ""record_date"": ""2024-06-15"",
                        ""exchange_rate"": ""5.478"",
                        ""currency"": ""Real""
                    }
                ]
            }";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                });

            var service = CreateService();
            var transactionDate = new DateTime(2024, 7, 15);

            // Act
            var (rate, rateDate) = await service.GetRateForDateAsync("BRL", transactionDate);

            // Assert
            Assert.Equal(5.478m, rate); // Deve parsear corretamente
            Assert.Equal(new DateTime(2024, 6, 15), rateDate);
        }

        [Fact]
        public async Task GetRateForDateAsync_Handles_Invalid_Date_Format()
        {
            // Arrange
            var responseJson = @"{
                ""data"": [
                    {
                        ""record_date"": ""invalid-date"",
                        ""exchange_rate"": ""5.50"",
                        ""currency"": ""Real""
                    }
                ]
            }";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                });

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<FormatException>(() =>
                service.GetRateForDateAsync("BRL", DateTime.UtcNow));
        }

        [Fact]
        public async Task GetRateForDateAsync_Handles_Invalid_Rate_Format()
        {
            // Arrange
            var responseJson = @"{
                ""data"": [
                    {
                        ""record_date"": ""2024-06-15"",
                        ""exchange_rate"": ""invalid-rate"",
                        ""currency"": ""Real""
                    }
                ]
            }";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                });

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<FormatException>(() =>
                service.GetRateForDateAsync("BRL", DateTime.UtcNow));
        }
    }
}