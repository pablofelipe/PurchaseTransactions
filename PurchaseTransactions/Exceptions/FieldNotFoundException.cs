namespace PurchaseTransactions.Exceptions
{
    public class FieldNotFoundException : Exception
    {
        public string FieldName { get; }

        public FieldNotFoundException(string fieldName)
            : base($"{fieldName} field not found in response")
        {
            FieldName = fieldName;
        }
    }
}
