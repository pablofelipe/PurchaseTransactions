namespace PurchaseTransactions.Domain.Dto;

public class CreateTransactionDto
{
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal AmountUsd { get; set; }
}
