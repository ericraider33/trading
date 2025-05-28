# Options Price Finder - Architecture Design

## Overview
This command-line application will fetch current stock prices, calculate target option strike prices, and retrieve options data for specified stock symbols. The output will be formatted as CSV.

## Components

### 1. Program Structure
- **Program.cs**: Entry point, argument parsing, and orchestration
- **Models/**: Data models for stocks, options, and program output
- **Services/**: Core business logic services
- **Utils/**: Helper utilities for date calculations, CSV generation, etc.

### 2. Key Services

#### StockService
- Responsible for fetching current stock prices
- Methods:
  - `GetCurrentPrice(string symbol)`: Returns the latest price for a given stock symbol

#### OptionsService
- Responsible for fetching options data
- Methods:
  - `GetCallOptionPrice(string symbol, DateTime expirationDate, decimal strikePrice)`: Returns the price of a call option

#### DateCalculator
- Handles date-related calculations
- Methods:
  - `GetNextFriday(DateTime fromDate)`: Calculates the next Friday from a given date

#### CsvGenerator
- Handles CSV output generation
- Methods:
  - `GenerateCsv(List<OptionResult> results, string outputPath)`: Creates CSV file from results

### 3. Data Models

#### StockPrice
- Properties:
  - `Symbol`: Stock ticker symbol
  - `CurrentPrice`: Latest price
  - `RetrievalTime`: When the price was fetched

#### OptionData
- Properties:
  - `Symbol`: Stock ticker symbol
  - `StrikePrice`: Strike price of the option
  - `ExpirationDate`: Expiration date
  - `CallPrice`: Price of the call option

#### OptionResult (Output Model)
- Properties:
  - `Symbol`: Stock ticker symbol
  - `CurrentPrice`: Current stock price
  - `FridayDate`: Date of the Friday (YYYY-MM-DD)
  - `CallOption1Percent`: Call option price at 1% above current price
  - `CallOption2Percent`: Call option price at 2% above current price
  - `CallOption3Percent`: Call option price at 3% above current price

## Flow

1. Parse command-line arguments (stock symbols)
2. For each symbol:
   - Fetch current stock price
   - Calculate target prices (current + 1%, 2%, 3%)
   - Calculate next Friday's date
   - Fetch call option prices for each target price on Friday
   - Store results
3. Generate CSV output with all results
4. Display success message with output file location

## API Integration

- Will use Polygon.io REST API for both stock prices and options data
- API key will be stored in appsettings.json or as an environment variable
- Will implement retry logic and error handling for API calls

## Error Handling

- Validate input symbols
- Handle API errors gracefully
- Provide meaningful error messages
- Log errors for debugging

## Configuration

- API keys and endpoints in appsettings.json
- Command-line parameters for:
  - Input symbols (required)
  - Output file path (optional, default: ./options_prices.csv)
  - Percentage increments (optional, default: 1%, 2%, 3%)
