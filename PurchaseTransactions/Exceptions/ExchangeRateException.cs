namespace PurchaseTransactions.Exceptions
{
    public class ExchangeRateException : Exception
    {
        public ExchangeRateException() { }
        public ExchangeRateException(string message) : base(message) { }
        public ExchangeRateException(string message, Exception inner) : base(message, inner) { }
    }
}
