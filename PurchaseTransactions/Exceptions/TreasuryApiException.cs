using System.Net;

namespace PurchaseTransactions.Exceptions
{
    public class TreasuryApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public TreasuryApiException(HttpStatusCode statusCode)
            : base($"Error querying Treasury API: {statusCode}")
        {
            StatusCode = statusCode;
        }

        public TreasuryApiException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
