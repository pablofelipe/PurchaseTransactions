namespace PurchaseTransactions.Exceptions
{
    public class ExchangeRateNotFoundException : Exception
    {
        public string Currency { get; }
        public DateTime TransactionDate { get; }

        public ExchangeRateNotFoundException(string currency, DateTime transactionDate)
            : base($"No exchange rate available for {currency} within 6 months prior to {transactionDate:yyyy-MM-dd}")
        {
            Currency = currency;
            TransactionDate = transactionDate;
        }
    }
}
