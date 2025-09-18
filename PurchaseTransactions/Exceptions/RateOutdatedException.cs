namespace PurchaseTransactions.Exceptions
{
    public class RateOutdatedException : Exception
    {
        public string Currency { get; }

        public RateOutdatedException(string currency)
            : base($"There is no rate available within the previous 6 months for {currency}")
        {
            Currency = currency;
        }
    }
}
