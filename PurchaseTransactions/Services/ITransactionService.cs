using PurchaseTransactions.Domain;
using PurchaseTransactions.Domain.Dto;

namespace PurchaseTransactions.Services;

public interface ITransactionService
{
    Task<Transaction> CreateAsync(CreateTransactionDto dto);
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<TransactionResponseDto> GetTransactionWithConversionAsync(Guid id, string currency);
}
