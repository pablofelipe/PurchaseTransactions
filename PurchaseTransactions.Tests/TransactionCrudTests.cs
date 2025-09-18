using Microsoft.AspNetCore.Http.HttpResults;
using PurchaseTransactions.Domain;
using System.ComponentModel.DataAnnotations;

namespace PurchaseTransactions.Tests
{
    public class TransactionCrudTests : BaseTests
    {
        [Fact]
        public void CreateTransaction_Should_Persist_Transaction()
        {
            // Arrange
            var service = GetService(nameof(CreateTransaction_Should_Persist_Transaction));
            var transaction = new Transaction
            {
                Description = "Notebook Purchase",
                TransactionDate = DateTime.UtcNow,
                AmountUsd = 1999.99m
            };

            // Act
            var created = service.CreateTransaction(transaction);
            var saved = service.GetById(created.Id);

            // Assert
            Assert.NotNull(saved);
            Assert.Equal("Notebook Purchase", saved.Description);
            Assert.Equal(1999.99m, saved.AmountUsd);
            Assert.Equal(created.TransactionDate, saved.TransactionDate);
        }

        [Fact]
        public void GetById_Should_Throw_When_Not_Found()
        {
            // Arrange
            var service = GetService(nameof(GetById_Should_Throw_When_Not_Found));

            // Act
            var newGuid = Guid.NewGuid();

            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() => service.GetById(newGuid));

            // Assert
            Assert.Equal($"Transaction with ID {newGuid} not found", exception.Message);
        }

        [Fact]
        public void GetAll_Should_Return_All_Transactions()
        {
            // Arrange
            var service = GetService(nameof(GetAll_Should_Return_All_Transactions));

            var transaction1 = new Transaction
            {
                Description = "Purchase 1",
                TransactionDate = DateTime.UtcNow,
                AmountUsd = 100.00m
            };

            var transaction2 = new Transaction
            {
                Description = "Purchase 2",
                TransactionDate = DateTime.UtcNow.AddDays(-1),
                AmountUsd = 200.00m
            };

            service.CreateTransaction(transaction1);
            service.CreateTransaction(transaction2);

            // Act
            var allTransactions = service.GetAll();

            // Assert
            Assert.Equal(2, allTransactions.Count());
        }

        [Fact]
        public void GetAll_Should_Return_Empty_List_When_No_Transactions()
        {
            // Arrange
            var service = GetService(nameof(GetAll_Should_Return_Empty_List_When_No_Transactions));

            // Act
            var allTransactions = service.GetAll();

            // Assert
            Assert.Empty(allTransactions);
        }

        [Fact]
        public void DeleteTransaction_Should_Throw_When_Transaction_NotFound()
        {
            // Arrange
            var service = GetService(nameof(DeleteTransaction_Should_Throw_When_Transaction_NotFound));
            var transaction = new Transaction
            {
                Description = "Purchase for remove",
                TransactionDate = DateTime.UtcNow,
                AmountUsd = 100.00m
            };

            var created = service.CreateTransaction(transaction);

            // Act
            var deleteResult = service.DeleteTransaction(created.Id);

            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() => service.GetById(created.Id));

            Assert.Equal($"Transaction with ID {created.Id} not found", exception.Message);
        }

        [Fact]
        public void DeleteTransaction_Should_Return_False_When_Not_Found()
        {
            // Arrange
            var service = GetService(nameof(DeleteTransaction_Should_Return_False_When_Not_Found));

            // Act
            var result = service.DeleteTransaction(Guid.NewGuid());

            // Assert
            Assert.False(result);
        }
    }
}