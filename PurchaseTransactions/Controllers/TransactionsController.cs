using Microsoft.AspNetCore.Mvc;
using PurchaseTransactions.Domain.Dto;
using PurchaseTransactions.Exceptions;
using PurchaseTransactions.Services;
using System.ComponentModel.DataAnnotations;

namespace PurchaseTransactions.Controllers;

[ApiController]
[Route("transactions")]
public class TransactionsController(ILogger<TransactionsController> logger, ITransactionService transactionService) : ControllerBase
{
    private readonly ITransactionService _transactionService = transactionService;
    private readonly ILogger<TransactionsController> _logger = logger;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
    {
        try
        {
            var txn = await _transactionService.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = txn.Id }, txn);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction");
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, [FromQuery] string? currency)
    {
        try
        {
            if (string.IsNullOrEmpty(currency))
            {
                var txn = await _transactionService.GetByIdAsync(id);
                if (txn == null)
                {
                    return NotFound($"Transaction with ID {id} not found");
                }
                return Ok(txn);
            }
            else
            {
                var result = await _transactionService.GetTransactionWithConversionAsync(id, currency);
                return Ok(result);
            }
        }
        catch (TransactionNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ExchangeRateNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidTransactionException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ExchangeRateServiceException ex)
        {
            _logger.LogError(ex, "Exchange rate service error");
            return StatusCode(500, "Service temporarily unavailable");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving transaction {Id}", id);
            return StatusCode(500, "An unexpected error occurred");
        }
    }
}
