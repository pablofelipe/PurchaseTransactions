namespace PurchaseTransactions.Exceptions
{
    public class NoRatesFoundException : Exception
    {
        public string Currency { get; }
        public DateTime TransactionDate { get; }

        public NoRatesFoundException(string currency, DateTime transactionDate)
            : base($"No rates found for {currency} until {transactionDate:yyyy-MM-dd}")
        {
            Currency = currency;
            TransactionDate = transactionDate;
        }
    }
}
