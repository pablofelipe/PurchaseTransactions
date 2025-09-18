namespace PurchaseTransactions.Exceptions
{
    public class ExchangeRateServiceException : Exception
    {
        public ExchangeRateServiceException(string message) : base(message) { }
    }
}
