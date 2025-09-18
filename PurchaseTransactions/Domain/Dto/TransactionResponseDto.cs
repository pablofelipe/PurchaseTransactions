namespace PurchaseTransactions.Domain.Dto
{
    public class TransactionResponseDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal AmountUsd { get; set; }
        public string TargetCurrency { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
        public decimal ConvertedAmount { get; set; }
        public DateTime ExchangeRateDate { get; set; }
    }
}
