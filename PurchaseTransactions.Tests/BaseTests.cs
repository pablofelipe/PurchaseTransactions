using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

            var mockRateService = new Mock<IExchangeRateService>();
            var context = new ApplicationDbContext(options);

            return new TransactionService(context, mockRateService.Object);
        }

        protected static (TransactionsController, Mock<ITransactionService>) CreateControllerWithMocks()
        {
            var logger = new Mock<ILogger<TransactionsController>>();
            var mockTxService = new Mock<ITransactionService>();
            var controller = new TransactionsController(logger.Object, mockTxService.Object);

            return (controller, mockTxService);
        }
    }
}
