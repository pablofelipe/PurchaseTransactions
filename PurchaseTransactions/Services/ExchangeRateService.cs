using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

namespace PurchaseTransactions.Services;

public class ExchangeRateService(IHttpClientFactory factory, IConfiguration configuration) : IExchangeRateService
{
    private readonly HttpClient _client = factory.CreateClient();
    private string BaseUrl = configuration["FiscalApi:BaseUrl"];

    public async Task<(decimal rate, DateTime rateDate)> GetRateForDateAsync(string currency, DateTime transactionDate)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency code is required");

        string filter = $"?filter=currency:eq:{currency}&sort=-record_date&format=json";
        var url = BaseUrl + filter;

        var response = await _client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Error querying Treasury API: {response.StatusCode}");

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        if (!doc.RootElement.TryGetProperty("data", out var data) || data.GetArrayLength() == 0)
            throw new Exception($"No rates found for {currency} until {transactionDate:yyyy-MM-dd}");

        var first = data[0];

        if (!first.TryGetProperty("record_date", out var recordDateProp))
            throw new Exception("Record_date field not found in response");

        if (!first.TryGetProperty("exchange_rate", out var rateProp))
            throw new Exception("Exchange_rate field not found in response");

        var rateDate = DateTime.ParseExact(recordDateProp.GetString()!, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var rate = decimal.Parse(rateProp.GetString()!, CultureInfo.InvariantCulture);

        if ((transactionDate - rateDate).TotalDays > 183) // ~6 meses
            throw new Exception($"There is no rate available within the previous 6 months for {currency}");

        return (rate, rateDate);
    }
}
