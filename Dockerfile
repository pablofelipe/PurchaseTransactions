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

# Generate self-signed certificate
RUN dotnet dev-certs https -ep /app/aspnetapp.pfx -p "U3Zg~Y80~LaK"

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install SQLite runtime and required packages
RUN apt-get update && \
    apt-get install -y sqlite3 libsqlite3-dev && \
    rm -rf /var/lib/apt/lists/*

# Create directory for certificates
RUN mkdir -p /app/certs

# Copy from build stage
COPY --from=build /app/publish . 
COPY --from=build /app/aspnetapp.pfx /app/certs/aspnetapp.pfx

# Expose ports
EXPOSE 80
EXPOSE 443

# Set environment variables for certificate
ENV ASPNETCORE_URLS=https://+:443;http://+:80
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="U3Zg~Y80~LaK"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/app/certs/aspnetapp.pfx"

# Set entrypoint
ENTRYPOINT ["dotnet", "PurchaseTransactions.dll"]


# Test stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS test
WORKDIR /src
COPY . .
RUN dotnet test PurchaseTransactions.Tests/PurchaseTransactions.Tests.csproj
