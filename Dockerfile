# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY PurchaseTransactions/PurchaseTransactions.csproj PurchaseTransactions/
COPY PurchaseTransactions.Tests/PurchaseTransactions.Tests.csproj PurchaseTransactions.Tests/

# Restore dependencies
RUN dotnet restore PurchaseTransactions/PurchaseTransactions.csproj
RUN dotnet restore PurchaseTransactions.Tests/PurchaseTransactions.Tests.csproj

# Copy everything else
COPY . .

# Build the project
RUN dotnet build PurchaseTransactions/PurchaseTransactions.csproj -c Release -o /app/build

# Publish the project
RUN dotnet publish PurchaseTransactions/PurchaseTransactions.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install SQLite runtime (if needed)
RUN apt-get update && apt-get install -y sqlite3 libsqlite3-dev && rm -rf /var/lib/apt/lists/*

# Copy from build stage
COPY --from=build /app/publish .

# Expose port
EXPOSE 80
EXPOSE 443

# Set entrypoint
ENTRYPOINT ["dotnet", "PurchaseTransactions.dll"]

# Estágio adicional para testes 
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS test
WORKDIR /src
COPY . .

