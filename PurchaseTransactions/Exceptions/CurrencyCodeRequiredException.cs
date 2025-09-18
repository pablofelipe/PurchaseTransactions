namespace PurchaseTransactions.Exceptions
{
    public class CurrencyCodeRequiredException : ExchangeRateException
    {
        public CurrencyCodeRequiredException() : base("Currency code is required") { }
    }
}
