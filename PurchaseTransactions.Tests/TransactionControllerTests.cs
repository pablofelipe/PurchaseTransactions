using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PurchaseTransactions.Controllers;
using PurchaseTransactions.Domain;
using PurchaseTransactions.Domain.Dto;
using PurchaseTransactions.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace PurchaseTransactions.Tests
{
    public class TransactionsControllerTests : BaseTests
    {
        [Fact]
        public async Task Get_WithCurrency_Should_Return_Converted_Transaction()
        {
            // Arrange
            var (controller, mockTxService) = CreateControllerWithMocks();
            var transactionId = Guid.NewGuid();
            var transactionDate = DateTime.UtcNow.AddDays(-30);

            mockTxService.Setup(x => x.GetTransactionWithConversionAsync(transactionId, "BRL"))
                .ReturnsAsync(new TransactionResponseDto
                {
                    Id = transactionId,
                    Description = "Test Transaction",
                    TransactionDate = transactionDate,
                    AmountUsd = 100.00m,
                    TargetCurrency = "BRL",
                    ExchangeRate = 5.50m,
                    ConvertedAmount = 550.00m,
                    ExchangeRateDate = transactionDate
                });

            // Act
            var result = await controller.Get(transactionId, "BRL");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var convertedResult = Assert.IsType<TransactionResponseDto>(okResult.Value);

            Assert.Equal(550.00m, convertedResult.ConvertedAmount);
            Assert.Equal(5.50m, convertedResult.ExchangeRate);
            Assert.Equal("BRL", convertedResult.TargetCurrency);
        }

        [Fact]
        public async Task Get_WithoutCurrency_Should_Return_Original_Transaction()
        {
            // Arrange
            var (controller, mockTxService) = CreateControllerWithMocks();
            var transactionId = Guid.NewGuid();

            var transaction = new Transaction
            {
                Id = transactionId,
                Description = "Test Transaction",
                TransactionDate = DateTime.UtcNow,
                AmountUsd = 100.00m
            };

            mockTxService.Setup(x => x.GetByIdAsync(transactionId))
                .ReturnsAsync(transaction);

            // Act
            var result = await controller.Get(transactionId, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTransaction = Assert.IsType<Transaction>(okResult.Value);
            Assert.Equal(transactionId, returnedTransaction.Id);
        }

        [Fact]
        public async Task Get_WithCurrency_Should_Return_NotFound_When_Transaction_Not_Exists()
        {
            // Arrange
            var (controller, mockTxService) = CreateControllerWithMocks();
            var transactionId = Guid.NewGuid();

            mockTxService.Setup(x => x.GetTransactionWithConversionAsync(transactionId, "BRL"))
                .ThrowsAsync(new TransactionNotFoundException(transactionId));

            // Act
            var result = await controller.Get(transactionId, "BRL");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Transaction with ID {transactionId} not found", notFoundResult.Value);
        }

        [Fact]
        public async Task Get_WithCurrency_Should_Return_BadRequest_When_ExchangeRate_Error()
        {
            // Arrange
            var (controller, mockTxService) = CreateControllerWithMocks();
            var transactionId = Guid.NewGuid();
            var testDate = new DateTime(2024, 1, 15);

            mockTxService.Setup(x => x.GetTransactionWithConversionAsync(transactionId, "EUR"))
                .ThrowsAsync(new ExchangeRateNotFoundException("EUR", testDate));

            // Act
            var result = await controller.Get(transactionId, "EUR");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Contains("No exchange rate available for EUR", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task Get_WithCurrency_Should_Return_InternalServerError_When_Unexpected_Error()
        {
            // Arrange
            var (controller, mockTxService) = CreateControllerWithMocks();
            var transactionId = Guid.NewGuid();

            mockTxService.Setup(x => x.GetTransactionWithConversionAsync(transactionId, "EUR"))
                .ThrowsAsync(new Exception("Unexpected database error"));

            // Act
            var result = await controller.Get(transactionId, "EUR");

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("An unexpected error occurred", statusCodeResult.Value);
        }

        [Fact]
        public async Task Create_Should_Return_Created_With_Location_Header()
        {
            // Arrange
            var (controller, mockTxService) = CreateControllerWithMocks();
            var transactionId = Guid.NewGuid();
            var dto = new CreateTransactionDto
            {
                Description = "Test Transaction",
                TransactionDate = DateTime.UtcNow,
                AmountUsd = 100.00m
            };

            var createdTransaction = new Transaction
            {
                Id = transactionId,
                Description = dto.Description,
                TransactionDate = dto.TransactionDate,
                AmountUsd = dto.AmountUsd
            };

            mockTxService.Setup(x => x.CreateAsync(dto))
                .ReturnsAsync(createdTransaction);

            // Act
            var result = await controller.Create(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(TransactionsController.Get), createdResult.ActionName);
            Assert.Equal(transactionId, createdResult.RouteValues?["id"]);
            Assert.Equal(createdTransaction, createdResult.Value);
        }

        [Fact]
        public async Task Create_Should_Return_BadRequest_When_Validation_Fails()
        {
            // Arrange
            var (controller, mockTxService) = CreateControllerWithMocks();
            var dto = new CreateTransactionDto
            {
                Description = "Test Transaction",
                TransactionDate = DateTime.UtcNow,
                AmountUsd = -100.00m 
            };

            mockTxService.Setup(x => x.CreateAsync(dto))
                .ThrowsAsync(new ValidationException("Purchase value must be positive"));

            // Act
            var result = await controller.Create(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Purchase value must be positive", badRequestResult.Value);
        }
    }
}