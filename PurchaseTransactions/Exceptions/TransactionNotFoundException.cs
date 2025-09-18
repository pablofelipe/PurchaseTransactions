namespace PurchaseTransactions.Exceptions
{
    public class TransactionNotFoundException : Exception
    {
        public Guid TransactionId { get; }

        public TransactionNotFoundException(Guid transactionId)
            : base($"Transaction with ID {transactionId} not found")
        {
            TransactionId = transactionId;
        }
    }
}
