# Options Price Finder - README

## Overview
This command-line application fetches current stock prices, calculates option strike prices at 1%, 2%, and 3% above the current price, and retrieves call option prices for the next Friday. The results are saved as a CSV file.

## Requirements
- .NET 6.0 SDK or later
- Polygon.io API key (free tier available at https://polygon.io/)

## Setup
1. Clone or download the project
2. Open `appsettings.json` and replace `YOUR_API_KEY_HERE` with your Polygon.io API key
3. Build the project: `dotnet build`

## Usage
```
dotnet run
```

By default, the application will process the test symbols: AAPL, TSLA, and NFLX.

To specify custom symbols:
```
dotnet run -- MSFT AMZN GOOGL
```

## Output
The application generates a CSV file named `options_prices.csv` with the following columns:
- Symbol: Stock ticker symbol
- CurrentPrice: Latest stock price
- FridayDate: Date of the next Friday (YYYY-MM-DD)
- CallOption1Percent: Call option price at 1% above current price
- CallOption2Percent: Call option price at 2% above current price
- CallOption3Percent: Call option price at 3% above current price

## Project Structure
- `Program.cs`: Main entry point and orchestration
- `Models/`: Data models for stocks, options, and results
- `Services/`: Core business logic and API integration
- `Utils/`: Helper utilities for date calculations and CSV generation

## Notes
- The application uses the Polygon.io API to fetch stock prices and options data
- If options data is not available for a specific strike price, the value will be null in the CSV
- The application finds the closest available strike price to the calculated target prices
