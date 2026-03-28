# Market Data API

A robust REST API service built with .NET Core that provides real-time and historical price information for specific market assets. The service integrates with the Fintacharts platform using both REST and WebSocket APIs.

## Features

- **Clean Architecture**: Separation of concerns across Domain, Application, Infrastructure, and API layers.
- **REST API Integration**: Fetches supported market assets via Fintacharts REST API.
- **WebSocket Integration**: Maintains a persistent connection to the Fintacharts streaming API to receive real-time price updates.
- **Background Synchronization**: Uses a background hosted service to manage OAuth2 tokens, initial data loading, and continuous WebSocket listening.
- **Entity Framework Core**: Stores asset and price data in an MS SQL Server database.
- **Dockerized**: Fully containerized using Docker and Docker Compose for seamless deployment.

## Prerequisites

To run this application, you only need to have Docker installed on your machine:
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Configuration

Before running the application, you need to provide your Fintacharts API credentials. 
You can update these in the `MarketData.Api/appsettings.json` file, or pass them as environment variables in the `docker-compose.yml`.

Ensure the `Fintacharts` section is configured:
```json
"Fintacharts": {
  "RestApiBaseUrl": "[https://platform.fintacharts.com](https://platform.fintacharts.com)",
  "WsApiBaseUrl": "wss://[platform.fintacharts.com/api/streaming/ws/v1/realtime](https://platform.fintacharts.com/api/streaming/ws/v1/realtime)",
  "Username": "your_test_username",
  "Password": "your_test_password",
  "Realm": "fintacharts",
  "ClientId": "app-cli"
}
```

## API Endpoints

Once the application is running, it exposes the following REST endpoints on http://localhost:8080 (or http://localhost:80 depending on your Docker mapping):

**1. Get supported market assets**
Returns a list of all assets retrieved from the provider and stored in the database.
```http
GET /api/assets
```
**Response (200 OK)**
```json
[
    {
        "symbol": "AUDCAD",
        "description": "Australian Dollar to Canadian Dollar"
    },
]
```
**2. Get price information for a specific asset**
Returns the latest price and the timestamp of the last update for a given symbol.
```http
GET /api/assets/AUDCAD/price
```
**Response (200 OK)**
```json
{
    "symbol": "AUDCAD",
    "price": 0.95495000,
    "lastUpdated": "2026-03-27T22:59:05.1555462"
}
```
## Project Structure
- **MarketData.Domain:** Contains core business entities and repository interfaces.
- **MarketData.Application:** Contains business logic, DTOs, and external service interfaces.
- **MarketData.Infrastructure:** Implements data access (EF Core), database migrations, and Fintacharts HTTP/WebSocket clients.
- **MarketData.Api:** The entry point containing REST controllers, dependency injection configuration, and background services.
