using Microsoft.EntityFrameworkCore;
using Moq;
using PurchaseTransactions.Controllers;
using PurchaseTransactions.Persistence;
using PurchaseTransactions.Services;

namespace PurchaseTransactions.Tests
{
    public class BaseTests
    {
        protected static TransactionService GetService(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new ApplicationDbContext(options);
            return new TransactionService(context);
        }

        protected static (TransactionsController, Mock<ITransactionService>, Mock<IExchangeRateService>) CreateControllerWithMocks()
        {
            var mockTxService = new Mock<ITransactionService>();
            var mockRateService = new Mock<IExchangeRateService>();
            var controller = new TransactionsController(mockTxService.Object, mockRateService.Object);

            return (controller, mockTxService, mockRateService);
        }
    }
}
