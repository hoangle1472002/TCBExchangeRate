# Techcombank Exchange Rate Crawler

## 1. Introduction

This project is a .NET Core 8.0 console application combined with a Web API to fetch and serve foreign exchange rate data from Techcombank. It includes a background service that automatically crawls data for the past month when the application starts and exposes APIs for consumers to retrieve exchange rate information. A static HTML frontend (using only vanilla JavaScript) is also provided to view the data.

---

## 2. Setup to Run Project

### Prerequisites

- .NET 8 SDK
- PostgreSQL

### Update the following values in the `appsettings.json` file in the **API project**:

#### Database Connection

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=ExchangeRateDb;Username=youruser;Password=yourpassword"
}
```

## 3. Logic to Obtain Exchange Rate Data

### Auto Fetch on Startup

- When the application starts, it automatically retrieves foreign exchange rate data from Techcombank for the past week (from today to 7 days ago).

### Scheduled Fetching Using Quartz

- The application uses Quartz.NET to schedule a background job that automatically retrieves foreign exchange rate data from Techcombank.

- The scheduled job (ExchangeRateImportJob) is triggered daily at 00:00 (midnight). When the job runs, it fetches exchange rate data for the previous day

### Manual Fetch via API

- In addition to the scheduled job, the application supports on-demand fetching via an API: `POST /api/exchangerate/fetch?date=yyyy-MM-dd`
- Manually triggers the fetch process to retrieve exchange rate data from Techcombank for a specific date.
- Useful for backfilling or re-fetching data due to failures or corrections.

### Retrieve All Exchange Rates by Currency and Date

- Endpoint: `GET /api/exchangerate/snapshots?date=yyyy-MM-dd&currencyCode=XXX`
- Returns purchase/selling rates for cash, cheque, and transfer methods on the specified date.

### Get Available Currencies

- Endpoint: `GET /api/currencies`
- Returns a list of available currencies with their name and code.

---

## 4. Clean Code & Architecture

- Follows **Clean Architecture**
- Applies **SOLID principles**
- Layered structure: `Domain`, `Application`, `Infrastructure`, `API`, `Persistence`
- Use of `ILogger<T>` for logging instead of `Console.WriteLine`
- Asynchronous programming with `async/await`

---

## 5. Static HTML Frontend

### Technology Used

- Pure HTML5 + CSS3 + Vanilla JavaScript
- Fetches data from the API and displays it semantically (using `<table>`, `<section>`, etc.)

### How to Run

- Ensure the API project is running at `https://localhost:7138`
- Open the `index.html` using a local server (e.g., using Live Server)

### Sample Fetch Call (in JavaScript)

```javascript
let url = `${apiBase}/exchangerate/snapshots?date=${formattedDate}&currencyCode=${currencyCode}`;
fetch(url)
  .then((response) => response.json())
  .then((data) => renderData(data))
  .catch((err) => console.error(err));
```
