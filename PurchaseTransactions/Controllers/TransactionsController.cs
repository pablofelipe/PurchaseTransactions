using Microsoft.AspNetCore.Mvc;
using PurchaseTransactions.Domain.Dto;
using PurchaseTransactions.Services;

namespace PurchaseTransactions.Controllers;

[ApiController]
[Route("transactions")]
public class TransactionsController(ITransactionService txSvc, IExchangeRateService rateSvc) : ControllerBase
{
    private readonly ITransactionService _txSvc = txSvc;
    private readonly IExchangeRateService _rateSvc = rateSvc;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
    {
        var txn = await _txSvc.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = txn.Id }, txn);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, [FromQuery] string? currency)
    {
        var txn = await _txSvc.GetByIdAsync(id);
        if (txn == null) return NotFound();

        if (string.IsNullOrWhiteSpace(currency))
            return Ok(txn);

        var (rate, rateDate) = await _rateSvc.GetRateForDateAsync(currency, txn.TransactionDate);
        var converted = Math.Round(txn.AmountUsd * rate, 2, MidpointRounding.AwayFromZero);

        var result = new TransactionWithConversionDto
        {
            Id = txn.Id,
            Description = txn.Description,
            TransactionDate = txn.TransactionDate,
            AmountUsd = txn.AmountUsd,
            TargetCurrency = currency.ToUpperInvariant(),
            ExchangeRate = rate,
            ConvertedAmount = converted
        };

        return Ok(result);
    }
}
