namespace PurchaseTransactions.Services;

public interface IExchangeRateService
{
    Task<(decimal rate, DateTime rateDate)> GetRateForDateAsync(string currency, DateTime transactionDate);
}
