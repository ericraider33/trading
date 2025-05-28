# Fidelity Options Scraper - README

## Overview
This .NET application uses Playwright to automate browser interactions with Fidelity's website, extract stock and options data, and generate a CSV report with call option prices at 1%, 2%, and 3% above the current stock price for the next Friday.

## Requirements
- .NET 6.0 SDK or later
- Google Chrome browser
- Fidelity brokerage account

## Features
- Works with an already logged-in Fidelity session (recommended for security)
- Supports manual login if needed
- Extracts current stock prices from Fidelity
- Calculates target prices at 1%, 2%, and 3% above current price
- Finds the next Friday date (or Friday of next week if today is Friday)
- Extracts call option prices for the specified expiration date
- Generates a CSV with all required data

## Setup
1. Install .NET 6.0 SDK or later if not already installed
2. Install the Playwright browsers:
   ```
   dotnet tool install --global Microsoft.Playwright.CLI
   playwright install
   ```
3. Build the application:
   ```
   dotnet build
   ```

## Usage

### Using with an Existing Browser Session (Recommended)
1. Open Chrome with remote debugging enabled:
   - Windows: `chrome.exe --remote-debugging-port=9222`
   - Mac: `open -a "Google Chrome" --args --remote-debugging-port=9222`
   - Linux: `google-chrome --remote-debugging-port=9222`
2. Log in to your Fidelity account manually
3. Run the application:
   ```
   dotnet run
   ```
4. When prompted, choose 'y' to use the existing browser session
5. The application will connect to your browser and extract the data

### Using with Automated Login
1. Run the application:
   ```
   dotnet run
   ```
2. When prompted, choose 'n' to start a new browser session
3. Enter your Fidelity username and password when prompted
4. The application will log in and extract the data

### Specifying Custom Symbols
By default, the application processes AAPL, TSLA, and NFLX. To specify custom symbols:
```
dotnet run -- MSFT AMZN GOOGL
```

## Output
The application generates a CSV file named `options_prices.csv` with the following columns:
- Symbol: Stock ticker symbol
- CurrentPrice: Current stock price
- FridayDate: Date of the next Friday (YYYY-MM-DD)
- CallOption1Percent: Call option price at 1% above current price
- CallOption2Percent: Call option price at 2% above current price
- CallOption3Percent: Call option price at 3% above current price

## Security Notes
- Using an existing browser session is recommended for security as it avoids storing credentials in the application
- The application never stores your Fidelity credentials
- If you choose to use automated login, credentials are only kept in memory during execution

## Troubleshooting
- If you encounter issues connecting to an existing browser session, ensure Chrome is running with the remote debugging port enabled
- If option prices are not found, the application will return null values in the CSV
- For any other issues, check the console output for error messages
