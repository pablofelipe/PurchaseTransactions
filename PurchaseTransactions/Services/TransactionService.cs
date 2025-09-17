using Microsoft.EntityFrameworkCore;
using Polly;
using PurchaseTransactions.Domain;
using PurchaseTransactions.Domain.Dto;
using PurchaseTransactions.Persistence;
using System.ComponentModel.DataAnnotations;

namespace PurchaseTransactions.Services;

public class TransactionService(ApplicationDbContext db) : ITransactionService
{
    private readonly ApplicationDbContext _db = db;

    public async Task<Transaction> CreateAsync(CreateTransactionDto dto)
    {
        var transaction = MapToTransaction(dto);
        return await CreateTransactionCoreAsync(transaction, async: true);
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        return await _db.Transactions.FindAsync(id);
    }

    public Transaction CreateTransaction(Transaction transaction)
    {
        return CreateTransactionCoreAsync(transaction, async: false).GetAwaiter().GetResult();
    }

    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        return await CreateTransactionCoreAsync(transaction, async: true);
    }

    public Transaction? GetById(Guid id)
    {
        return _db.Transactions.FirstOrDefault(t => t.Id == id);
    }

    public IEnumerable<Transaction> GetAll()
    {
        return [.. _db.Transactions];
    }

    public async Task<IEnumerable<Transaction>> GetAllAsync()
    {
        return await _db.Transactions.ToListAsync();
    }

    // Método centralizado usando padrão Template Method
    private async Task<Transaction> CreateTransactionCoreAsync(Transaction transaction, bool async)
    {
        ValidateTransaction(transaction);
        ProcessTransaction(transaction);

        _db.Transactions.Add(transaction);

        if (async)
        {
            await _db.SaveChangesAsync();
        }
        else
        {
            _db.SaveChanges();
        }

        return transaction;
    }

    private void ValidateTransaction(Transaction transaction)
    {
        if (string.IsNullOrWhiteSpace(transaction.Description) || transaction.Description.Length > 50)
            throw new ValidationException("Invalid description");

        if (transaction.AmountUsd <= 0)
            throw new ValidationException("Purchase value must be positive");
    }

    private void ProcessTransaction(Transaction transaction)
    {
        transaction.Description = transaction.Description?.Trim();
        transaction.AmountUsd = Math.Round(transaction.AmountUsd, 2, MidpointRounding.AwayFromZero);

        if (transaction.Id == Guid.Empty)
        {
            transaction.Id = Guid.NewGuid();
        }
    }

    private Transaction MapToTransaction(CreateTransactionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Description) || dto.Description.Length > 50)
            throw new ArgumentException("Description is invalid.");
        if (dto.AmountUsd <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        return new Transaction
        {
            Description = dto.Description.Trim(),
            TransactionDate = dto.TransactionDate.Date,
            AmountUsd = Math.Round(dto.AmountUsd, 2, MidpointRounding.AwayFromZero)
        };
    }

    public bool DeleteTransaction(Guid id)
    {
        var transaction = _db.Transactions.Find(id);
        if (transaction == null)
        {
            return false;
        }

        _db.Remove(transaction);
        _db.SaveChanges();

        return true;
    }
}