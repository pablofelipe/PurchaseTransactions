using PurchaseTransactions.Exceptions;
using System.Globalization;
using System.Text.Json;

namespace PurchaseTransactions.Services;

public class ExchangeRateService(IHttpClientFactory factory, IConfiguration configuration) : IExchangeRateService
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly string urlBase = configuration["FiscalApi:BaseUrl"];

    public async Task<(decimal rate, DateTime rateDate)> GetRateForDateAsync(string currency, DateTime transactionDate)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new CurrencyCodeRequiredException();

        string filter = $"?filter=currency:eq:{currency}&sort=-record_date&format=json";
        var url = urlBase + filter;

        var response = await _client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            throw new TreasuryApiException(response.StatusCode);

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        if (!doc.RootElement.TryGetProperty("data", out var data) || data.GetArrayLength() == 0)
            throw new NoRatesFoundException(currency, transactionDate);

        var first = data[0];

        if (!first.TryGetProperty("record_date", out var recordDateProp))
            throw new FieldNotFoundException("record_date");

        if (!first.TryGetProperty("exchange_rate", out var rateProp))
            throw new FieldNotFoundException("exchange_rate");

        var rateDate = DateTime.ParseExact(recordDateProp.GetString()!, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var rate = decimal.Parse(rateProp.GetString()!, CultureInfo.InvariantCulture);

        if ((transactionDate - rateDate).TotalDays > 183) // ~6 meses
            throw new RateOutdatedException(currency);

        return (rate, rateDate);
    }
}
