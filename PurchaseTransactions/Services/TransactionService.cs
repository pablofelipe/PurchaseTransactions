using Microsoft.EntityFrameworkCore;
using Polly;
using PurchaseTransactions.Domain;
using PurchaseTransactions.Domain.Dto;
using PurchaseTransactions.Exceptions;
using PurchaseTransactions.Persistence;
using System.ComponentModel.DataAnnotations;

namespace PurchaseTransactions.Services;

public class TransactionService(ApplicationDbContext db, IExchangeRateService exchangeRateService) : ITransactionService
{
    private readonly ApplicationDbContext _db = db;
    private readonly IExchangeRateService _exchangeRateService = exchangeRateService;

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
        var transaction = _db.Transactions.FirstOrDefault(t => t.Id == id);

        return transaction ?? throw new ValidationException($"Transaction with ID {id} not found");
    }

    public IEnumerable<Transaction> GetAll()
    {
        return [.. _db.Transactions];
    }

    public async Task<IEnumerable<Transaction>> GetAllAsync()
    {
        return await _db.Transactions.ToListAsync();
    }

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

    private static void ValidateTransaction(Transaction transaction)
    {
        if (string.IsNullOrWhiteSpace(transaction.Description) || transaction.Description.Length > 50)
            throw new ValidationException("Invalid description");

        if (transaction.AmountUsd <= 0)
            throw new ValidationException("Purchase value must be positive");
    }

    private static void ProcessTransaction(Transaction transaction)
    {
        transaction.Description = transaction.Description?.Trim();
        transaction.AmountUsd = Math.Round(transaction.AmountUsd, 2, MidpointRounding.AwayFromZero);

        if (transaction.Id == Guid.Empty)
        {
            transaction.Id = Guid.NewGuid();
        }
    }

    private static Transaction MapToTransaction(CreateTransactionDto dto)
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

    public async Task<TransactionResponseDto> GetTransactionWithConversionAsync(Guid id, string currency)
    {
        var transaction = await _db.Transactions.FindAsync(id);
        if (transaction == null)
        {
            throw new TransactionNotFoundException(id);
        }

        try
        {
            var (rate, rateDate) = await _exchangeRateService.GetRateForDateAsync(
                currency, transaction.TransactionDate
            );

            var convertedAmount = transaction.AmountUsd * rate;

            return new TransactionResponseDto
            {
                Id = transaction.Id,
                Description = transaction.Description,
                TransactionDate = transaction.TransactionDate,
                AmountUsd = transaction.AmountUsd,
                TargetCurrency = currency.ToUpper(),
                ExchangeRate = rate,
                ConvertedAmount = Math.Round(convertedAmount, 2),
                ExchangeRateDate = rateDate
            };
        }
        catch (CurrencyCodeRequiredException)
        {
            throw new InvalidTransactionException("Currency code is required for conversion");
        }
        catch (TreasuryApiException ex)
        {
            throw new ExchangeRateServiceException($"Error communicating with exchange rate service: {ex.Message}");
        }
        catch (NoRatesFoundException)
        {
            throw new ExchangeRateNotFoundException(currency, transaction.TransactionDate);
        }
        catch (FieldNotFoundException ex)
        {
            throw new ExchangeRateServiceException($"Invalid response from exchange rate service: {ex.Message}");
        }
        catch (RateOutdatedException)
        {
            throw new ExchangeRateNotFoundException(currency, transaction.TransactionDate);
        }
        catch (ExchangeRateException ex)
        {
            throw new ExchangeRateServiceException($"Exchange rate error: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new ExchangeRateServiceException($"Unexpected error retrieving exchange rate: {ex.Message}");
        }
    }
}