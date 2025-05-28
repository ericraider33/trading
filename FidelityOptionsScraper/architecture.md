# Fidelity Options Scraper - Architecture Design

## Overview
This .NET application will use Playwright to automate browser interactions with Fidelity's website, extract stock and options data, and generate a CSV report with the required information.

## Components

### 1. Program Structure
- **Program.cs**: Entry point, argument parsing, and orchestration
- **Models/**: Data models for stocks, options, and program output
- **Services/**: Core business logic services
- **Utils/**: Helper utilities for date calculations, CSV generation, etc.
- **Scrapers/**: Playwright-based web scraping components

### 2. Key Services

#### BrowserService
- Responsible for browser automation with Playwright
- Methods:
  - `InitializeBrowser(bool useExistingSession)`: Sets up browser instance
  - `AttachToExistingSession()`: Connects to an already logged-in session
  - `Login(string username, string password)`: Handles Fidelity login if needed
  - `CloseBrowser()`: Cleanup resources

#### StockScraperService
- Responsible for extracting stock price data
- Methods:
  - `GetCurrentPrice(string symbol)`: Navigates to stock page and extracts price

#### OptionsScraperService
- Responsible for extracting options data
- Methods:
  - `GetCallOptionPrices(string symbol, DateTime expirationDate, List<decimal> strikePrices)`: Extracts option prices for given strikes

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
2. Initialize Playwright browser (with option to use existing session)
3. For each symbol:
   - Navigate to stock page and extract current price
   - Calculate target prices (current + 1%, 2%, 3%)
   - Calculate next Friday's date
   - Navigate to options chain page with the symbol and expiration date
   - Extract call option prices for each target price
   - Store results
4. Generate CSV output with all results
5. Display success message with output file location

## Browser Automation Strategy

### Session Handling
- Primary approach: Work with an already logged-in session
  - User launches browser and logs in manually
  - Application connects to the existing browser session
- Fallback approach: Automated login
  - Application launches browser and performs login
  - Credentials stored securely or provided at runtime

### Navigation Patterns
- Direct URL navigation to options chain using template:
  - `https://digital.fidelity.com/ftgw/digital/options-research/option-chain?symbol={SYMBOL}&oarchain=true`
- Handling of page loads and dynamic content
- Waiting for specific elements before extraction

## Error Handling

- Validate input symbols
- Handle navigation errors gracefully
- Provide meaningful error messages
- Implement retry logic for flaky elements
- Log errors for debugging

## Configuration

- Command-line parameters for:
  - Input symbols (required)
  - Output file path (optional, default: ./options_prices.csv)
  - Browser session mode (new or existing)
  - Headless mode toggle
- Optional configuration file for persistent settings
