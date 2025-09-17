using Microsoft.AspNetCore.Mvc;
using Moq;
using PurchaseTransactions.Controllers;
using PurchaseTransactions.Domain;
using PurchaseTransactions.Domain.Dto;

namespace PurchaseTransactions.Tests
{
    public class TransactionsControllerTests : BaseTests
    {
        [Fact]
        public async Task Get_WithCurrency_Should_Return_Converted_Transaction()
        {
            // Arrange
            var (controller, mockTxService, mockRateService) = CreateControllerWithMocks();
            var transactionId = Guid.NewGuid();
            var transactionDate = DateTime.UtcNow.AddDays(-30);

            var transaction = new Transaction
            {
                Id = transactionId,
                Description = "Test Transaction",
                TransactionDate = transactionDate,
                AmountUsd = 100.00m
            };

            mockTxService.Setup(x => x.GetByIdAsync(transactionId))
                .ReturnsAsync(transaction);

            mockRateService.Setup(x => x.GetRateForDateAsync("BRL", transactionDate))
                .ReturnsAsync((5.50m, transactionDate));

            // Act
            var result = await controller.Get(transactionId, "BRL");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var convertedResult = Assert.IsType<TransactionWithConversionDto>(okResult.Value);

            Assert.Equal(550.00m, convertedResult.ConvertedAmount);
            Assert.Equal(5.50m, convertedResult.ExchangeRate);
            Assert.Equal("BRL", convertedResult.TargetCurrency);
        }

        [Fact]
        public async Task Get_WithoutCurrency_Should_Return_Original_Transaction()
        {
            // Arrange
            var (controller, mockTxService, mockRateService) = CreateControllerWithMocks();
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
            var (controller, mockTxService, mockRateService) = CreateControllerWithMocks();
            var transactionId = Guid.NewGuid();

            mockTxService.Setup(x => x.GetByIdAsync(transactionId))
                .ReturnsAsync((Transaction?)null);

            // Act
            var result = await controller.Get(transactionId, "BRL");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Get_WithCurrency_Should_Propagate_ExchangeRate_Errors()
        {
            // Arrange
            var (controller, mockTxService, mockRateService) = CreateControllerWithMocks();
            var transactionId = Guid.NewGuid();
            var transactionDate = DateTime.UtcNow.AddMonths(-7);

            var transaction = new Transaction
            {
                Id = transactionId,
                Description = "Old Transaction",
                TransactionDate = transactionDate,
                AmountUsd = 100.00m
            };

            mockTxService.Setup(x => x.GetByIdAsync(transactionId))
                .ReturnsAsync(transaction);

            mockRateService.Setup(x => x.GetRateForDateAsync("EUR", transactionDate))
                .ThrowsAsync(new Exception("No rate available"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => controller.Get(transactionId, "EUR"));
        }

        [Fact]
        public async Task Create_Should_Return_Created_With_Location_Header()
        {
            // Arrange
            var (controller, mockTxService, mockRateService) = CreateControllerWithMocks();
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
        }
    }
}