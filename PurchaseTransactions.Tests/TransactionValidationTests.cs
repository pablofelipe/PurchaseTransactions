using PurchaseTransactions.Domain;
using System.ComponentModel.DataAnnotations;

namespace PurchaseTransactions.Tests
{
    public class TransactionValidationTests : BaseTests
    {
        [Fact]
        public void CreateTransaction_Should_Throw_When_Description_Exceeds_50_Characters()
        {
            // Arrange
            var service = GetService(nameof(CreateTransaction_Should_Throw_When_Description_Exceeds_50_Characters));
            var transaction = new Transaction
            {
                Description = "This description is longer than fifty characters, which is invalid.",
                TransactionDate = DateTime.UtcNow,
                AmountUsd = 100.00m
            };

            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() => service.CreateTransaction(transaction));

            Assert.Equal("Invalid description", exception.Message);
        }

        [Fact]
        public void CreateTransaction_Should_Throw_When_Description_Is_Null()
        {
            // Arrange
            var service = GetService(nameof(CreateTransaction_Should_Throw_When_Description_Is_Null));
            var transaction = new Transaction
            {
                Description = null,
                TransactionDate = DateTime.UtcNow,
                AmountUsd = 100.00m
            };

            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() => service.CreateTransaction(transaction));

            Assert.Equal("Invalid description", exception.Message);
        }

        [Fact]
        public void CreateTransaction_Should_Throw_When_Amount_Is_Negative()
        {
            // Arrange
            var service = GetService(nameof(CreateTransaction_Should_Throw_When_Amount_Is_Negative));
            var transaction = new Transaction
            {
                Description = "Valid Purchase",
                TransactionDate = DateTime.UtcNow,
                AmountUsd = -100.00m
            };

            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() => service.CreateTransaction(transaction));

            Assert.Equal("Purchase value must be positive", exception.Message);
        }

        [Fact]
        public void CreateTransaction_Should_Throw_When_Amount_Is_Zero()
        {
            // Arrange
            var service = GetService(nameof(CreateTransaction_Should_Throw_When_Amount_Is_Zero));
            var transaction = new Transaction
            {
                Description = "Valid Purchase",
                TransactionDate = DateTime.UtcNow,
                AmountUsd = 0.00m
            };

            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() => service.CreateTransaction(transaction));

            Assert.Equal("Purchase value must be positive", exception.Message);
        }

        [Fact]
        public void CreateTransaction_Should_Round_Amount_To_Nearest_Cent()
        {
            // Arrange
            var service = GetService(nameof(CreateTransaction_Should_Round_Amount_To_Nearest_Cent));
            var transaction = new Transaction
            {
                Description = "Decimal Purchase",
                TransactionDate = DateTime.UtcNow,
                AmountUsd = 123.456789m
            };

            // Act
            var created = service.CreateTransaction(transaction);
            var saved = service.GetById(created.Id);

            // Assert
            Assert.Equal(123.46m, saved.AmountUsd);
        }

        [Fact]
        public void CreateTransaction_Should_Assign_Unique_Id()
        {
            // Arrange
            var service = GetService(nameof(CreateTransaction_Should_Assign_Unique_Id));
            var transaction1 = new Transaction
            {
                Description = "Purchase 1",
                TransactionDate = DateTime.UtcNow,
                AmountUsd = 100.00m
            };

            var transaction2 = new Transaction
            {
                Description = "Purchase 2",
                TransactionDate = DateTime.UtcNow,
                AmountUsd = 200.00m
            };

            // Act
            var created1 = service.CreateTransaction(transaction1);
            var created2 = service.CreateTransaction(transaction2);

            // Assert
            Assert.NotEqual(created1.Id, created2.Id);
            Assert.NotEqual(Guid.Empty, created1.Id);
            Assert.NotEqual(Guid.Empty, created2.Id);
        }
    }
}
