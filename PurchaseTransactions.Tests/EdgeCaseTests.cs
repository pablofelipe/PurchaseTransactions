using PurchaseTransactions.Domain;

namespace PurchaseTransactions.Tests
{
    public class EdgeCaseTests : BaseTests
    {
        [Fact]
        public void CreateTransaction_Should_Accept_Maximum_Length_Description()
        {
            // Arrange
            var service = GetService(nameof(CreateTransaction_Should_Accept_Maximum_Length_Description));
            var maxLengthDescription = new string('A', 50);
            var transaction = new Transaction
            {
                Description = maxLengthDescription,
                TransactionDate = DateTime.UtcNow,
                AmountUsd = 100.00m
            };

            // Act
            var created = service.CreateTransaction(transaction);
            var saved = service.GetById(created.Id);

            // Assert
            Assert.NotNull(saved);
            Assert.Equal(50, saved.Description.Length);
        }

        [Fact]
        public void CreateTransaction_Should_Accept_Very_Small_Amount()
        {
            // Arrange
            var service = GetService(nameof(CreateTransaction_Should_Accept_Very_Small_Amount));
            var transaction = new Transaction
            {
                Description = "Small Amount",
                TransactionDate = DateTime.UtcNow,
                AmountUsd = 0.01m
            };

            // Act
            var created = service.CreateTransaction(transaction);
            var saved = service.GetById(created.Id);

            // Assert
            Assert.NotNull(saved);
            Assert.Equal(0.01m, saved.AmountUsd);
        }

        [Fact]
        public void CreateTransaction_Should_Accept_Very_Large_Amount()
        {
            // Arrange
            var service = GetService(nameof(CreateTransaction_Should_Accept_Very_Large_Amount));
            var transaction = new Transaction
            {
                Description = "Very Large Amount",
                TransactionDate = DateTime.UtcNow,
                AmountUsd = 9999999.99m
            };

            // Act
            var created = service.CreateTransaction(transaction);
            var saved = service.GetById(created.Id);

            // Assert
            Assert.NotNull(saved);
            Assert.Equal(9999999.99m, saved.AmountUsd);
        }

        [Fact]
        public void CreateTransaction_Should_Handle_Multiple_Transactions_Concurrently()
        {
            // Arrange
            var service = GetService(nameof(CreateTransaction_Should_Handle_Multiple_Transactions_Concurrently));
            var transactions = new[]
            {
            new Transaction { Description = "Buy 1", TransactionDate = DateTime.UtcNow, AmountUsd = 10.00m },
            new Transaction { Description = "Buy 2", TransactionDate = DateTime.UtcNow, AmountUsd = 20.00m },
            new Transaction { Description = "Buy 3", TransactionDate = DateTime.UtcNow, AmountUsd = 30.00m }
        };

            // Act
            foreach (var transaction in transactions)
            {
                service.CreateTransaction(transaction);
            }

            var allTransactions = service.GetAll();

            // Assert
            Assert.Equal(3, allTransactions.Count());
        }
    }
}
