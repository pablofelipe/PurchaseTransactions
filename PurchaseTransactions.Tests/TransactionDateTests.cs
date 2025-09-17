using PurchaseTransactions.Domain;

namespace PurchaseTransactions.Tests
{
    public class TransactionDateTests : BaseTests
    {
        [Fact]
        public void CreateTransaction_Should_Accept_Future_Date()
        {
            // Arrange
            var service = GetService(nameof(CreateTransaction_Should_Accept_Future_Date));
            var futureDate = DateTime.UtcNow.AddDays(1);
            var transaction = new Transaction
            {
                Description = "Future Purchase",
                TransactionDate = futureDate,
                AmountUsd = 100.00m
            };

            // Act
            var created = service.CreateTransaction(transaction);
            var saved = service.GetById(created.Id);

            // Assert
            Assert.NotNull(saved);
            Assert.Equal(futureDate.Date, saved.TransactionDate.Date);
        }

        [Fact]
        public void CreateTransaction_Should_Accept_Past_Date()
        {
            // Arrange
            var service = GetService(nameof(CreateTransaction_Should_Accept_Past_Date));
            var pastDate = DateTime.UtcNow.AddYears(-1);
            var transaction = new Transaction
            {
                Description = "Past Purchase",
                TransactionDate = pastDate,
                AmountUsd = 100.00m
            };

            // Act
            var created = service.CreateTransaction(transaction);
            var saved = service.GetById(created.Id);

            // Assert
            Assert.NotNull(saved);
            Assert.Equal(pastDate.Date, saved.TransactionDate.Date);
        }

        [Fact]
        public void CreateTransaction_Should_Store_Date_With_Time()
        {
            // Arrange
            var service = GetService(nameof(CreateTransaction_Should_Store_Date_With_Time));
            var specificDateTime = new DateTime(2024, 1, 15, 14, 30, 45);
            var transaction = new Transaction
            {
                Description = "Time-specific purchase",
                TransactionDate = specificDateTime,
                AmountUsd = 100.00m
            };

            // Act
            var created = service.CreateTransaction(transaction);
            var saved = service.GetById(created.Id);

            // Assert
            Assert.NotNull(saved);
            Assert.Equal(specificDateTime, saved.TransactionDate);
        }
    }
}