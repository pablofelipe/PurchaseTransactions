# Purchase Transactions API

A .NET Core API for managing purchase transactions with real-time currency conversion using the U.S. Treasury Department's exchange rate data.

## 📋 Overview

This API allows you to create purchase transactions in USD and convert them to various currencies using real exchange rates from the U.S. Treasury Department's fiscal data API. It's built with .NET Core, uses SQLite for data storage, and is fully containerized with Docker.

## ✨ Features

- **Transaction Management**: Create and retrieve purchase transactions
- **Real-time Currency Conversion**: Convert USD amounts to various currencies using official exchange rates
- **RESTful API**: Clean, intuitive API endpoints
- **Docker Support**: Easy deployment with Docker and Docker Compose
- **SQLite Database**: Lightweight, file-based database storage
- **Unit Tests**: Comprehensive test coverage

## 🚀 API Endpoints

### Local Development (without Docker)
- **HTTP**: http://localhost:5097
- **HTTPS**: https://localhost:7082

### Docker Deployment
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001

### Example with Docker:

```bash
# Create transaction
curl -X 'POST' \
  'http://localhost:5000/transactions' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "description": "Test Purchase",
  "transactionDate": "2025-09-17T00:00:00Z",
  "amountUsd": 29.87
}'

# Get converted transaction
curl -X 'GET' \
  'http://localhost:5000/transactions/940f4ff6-fc8c-4649-a42f-b4ab1f6fe421?currency=Real' \
  -H 'accept: */*'
  
### Create a Transaction
```bash
POST /transactions
Content-Type: application/json

{
  "description": "Test Purchase",
  "transactionDate": "2025-09-17T00:00:00Z",
  "amountUsd": 29.87
}
```

**Response:**
```json
{
  "id": "940f4ff6-fc8c-4649-a42f-b4ab1f6fe421",
  "description": "Test Purchase",
  "transactionDate": "2025-09-17T00:00:00Z",
  "amountUsd": 29.87
}
```

### Get Transaction with Currency Conversion
```bash
GET /transactions/{id}?currency={currencyCode}
```

**Example:**
```bash
GET /transactions/940f4ff6-fc8c-4649-a42f-b4ab1f6fe421?currency=Real
```

**Response:**
```json
{
  "id": "940f4ff6-fc8c-4649-a42f-b4ab1f6fe421",
  "description": "Test Purchase",
  "transactionDate": "2025-09-17T00:00:00",
  "amountUsd": 29.87,
  "targetCurrency": "REAL",
  "exchangeRate": 5.478,
  "convertedAmount": 163.63
}
```

## 🛠️ Technology Stack

- **.NET Core 8.0**: Backend framework
- **Entity Framework Core**: ORM for database operations
- **SQLite**: Database storage
- **Docker**: Containerization
- **xUnit**: Unit testing framework
- **HttpClient**: External API integration

## 📦 Installation & Setup

### Prerequisites
- .NET 8.0 SDK
- Docker and Docker Compose (optional)
- Git

### Method 1: Using Docker (Recommended)

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/PurchaseTransactions.git
   cd PurchaseTransactions
   ```

2. **Build and run with Docker Compose**
   ```bash
   docker-compose up -d
   ```

### Method 2: Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/PurchaseTransactions.git
   cd PurchaseTransactions
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the application**
   ```bash
   dotnet run --project PurchaseTransactions/PurchaseTransactions.csproj
   ```

4. **Run tests**
   ```bash
   dotnet test PurchaseTransactions.Tests/PurchaseTransactions.Tests.csproj
   ```

## 🐳 Docker Commands

### Build and run the application
```bash
docker-compose up -d purchasetransactions
```

### Run tests
```bash
docker-compose run --rm tests
```

### View logs
```bash
docker-compose logs -f purchasetransactions
```

### Stop the application
```bash
docker-compose down
```

## 🔧 Configuration

The application uses `appsettings.json` for configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=transactions.db"
  },
  "ApiFiscal": {
    "BaseUrl": "https://api.fiscaldata.treasury.gov/services/api/fiscal_service/v1/accounting/od/rates_of_exchange",
    "Timeout": 30
  }
}
```

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Development/Production
- `ConnectionStrings__DefaultConnection`: Database connection string
- `ApiFiscal__BaseUrl`: Treasury API endpoint

## 📊 External API Integration

This application integrates with the U.S. Treasury Department's Fiscal Data API to retrieve real exchange rates:

**API Endpoint:** `https://api.fiscaldata.treasury.gov/services/api/fiscal_service/v1/accounting/od/rates_of_exchange`

The API returns exchange rate data in the following format:
```json
{
  "data": [
    {
      "record_date": "2001-03-31",
      "country": "Brazil",
      "currency": "Real",
      "exchange_rate": "2.043",
      "effective_date": "2001-03-31"
    }
  ]
}
```

## 🧪 Testing

### Run all tests
```bash
dotnet test
```

### Run tests with Docker
```bash
docker-compose run --rm tests
```

### Test coverage includes:
- Transaction creation and retrieval
- Currency conversion logic
- API integration tests
- Error handling scenarios

## 📁 Project Structure

```
PurchaseTransactions/
├── PurchaseTransactions/          # Main API project
│   ├── Controllers/              # API controllers
│   ├── Domain/              	  # Business Rules
│   ├── Domain/Dto                # Data Transfer Object
│   ├── Persistence/              # Database context 
│   ├── Services/                 # Logic services
│   └── appsettings.json          # Configuration
├── PurchaseTransactions.Tests/   # Unit test project
├── Dockerfile                    # Docker configuration
├── docker-compose.yml           # Docker Compose setup
└── README.md                    # This file
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

If you encounter any issues or have questions:

1. Check the [Issues](../../issues) page
2. Create a new issue with detailed description
3. Provide steps to reproduce if it's a bug

## 🔗 Useful Links

- [U.S. Treasury Fiscal Data API](https://fiscaldata.treasury.gov/api-documentation/)
- [.NET Core Documentation](https://docs.microsoft.com/en-us/dotnet/core/)
- [Docker Documentation](https://docs.docker.com/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

---

**Note**: This API uses real exchange rate data from the U.S. Treasury Department. The exchange rates are updated periodically and reflect official government data.