using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FidelityOptionsScraper.Models;
using FidelityOptionsScraper.Scrapers;
using FidelityOptionsScraper.Services;
using FidelityOptionsScraper.Utils;

namespace FidelityOptionsScraper
{
    public class OptionsCalculationService
    {
        private readonly StockScraperService _stockScraper;
        private readonly OptionsScraperService _optionsScraper;

        public OptionsCalculationService(StockScraperService stockScraper, OptionsScraperService optionsScraper)
        {
            _stockScraper = stockScraper;
            _optionsScraper = optionsScraper;
        }

        /// <summary>
        /// Calculates options data for a given symbol
        /// </summary>
        /// <param name="symbol">The stock symbol</param>
        /// <returns>OptionResult with all required data</returns>
        public async Task<OptionResult> CalculateOptionsForSymbolAsync(string symbol)
        {
            try
            {
                Console.WriteLine($"Processing {symbol}...");
                
                // Get current stock price
                var stockPrice = await _stockScraper.GetCurrentPriceAsync(symbol);
                Console.WriteLine($"Current price for {symbol}: ${stockPrice.CurrentPrice}");
                
                if (stockPrice.CurrentPrice <= 0)
                {
                    Console.WriteLine($"Warning: Invalid price for {symbol}. Skipping options calculation.");
                    return new OptionResult
                    {
                        Symbol = symbol,
                        CurrentPrice = 0,
                        FridayDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        CallOption1Percent = null,
                        CallOption2Percent = null,
                        CallOption3Percent = null
                    };
                }
                
                // Calculate target prices (current + 1%, 2%, 3%)
                decimal price1Percent = Math.Round(stockPrice.CurrentPrice * 1.01m, 2);
                decimal price2Percent = Math.Round(stockPrice.CurrentPrice * 1.02m, 2);
                decimal price3Percent = Math.Round(stockPrice.CurrentPrice * 1.03m, 2);
                
                Console.WriteLine($"Target prices: +1%=${price1Percent}, +2%=${price2Percent}, +3%=${price3Percent}");
                
                // Calculate next Friday
                DateTime today = DateTime.Today;
                DateTime nextFriday = DateCalculator.GetNextFriday(today);
                string fridayDate = nextFriday.ToString("yyyy-MM-dd");
                
                Console.WriteLine($"Next Friday: {fridayDate}");
                
                // Get options prices for each target price
                var targetPrices = new List<decimal> { price1Percent, price2Percent, price3Percent };
                var optionsData = await _optionsScraper.GetCallOptionPricesAsync(symbol, nextFriday, targetPrices);
                
                // Find the option prices for each target
                decimal? callOption1Percent = null;
                decimal? callOption2Percent = null;
                decimal? callOption3Percent = null;
                
                foreach (var option in optionsData)
                {
                    if (Math.Abs(option.StrikePrice - price1Percent) < 0.01m)
                    {
                        callOption1Percent = option.CallPrice;
                    }
                    else if (Math.Abs(option.StrikePrice - price2Percent) < 0.01m)
                    {
                        callOption2Percent = option.CallPrice;
                    }
                    else if (Math.Abs(option.StrikePrice - price3Percent) < 0.01m)
                    {
                        callOption3Percent = option.CallPrice;
                    }
                }
                
                Console.WriteLine($"Call option prices: +1%=${callOption1Percent}, +2%=${callOption2Percent}, +3%=${callOption3Percent}");
                
                // Create result
                return new OptionResult
                {
                    Symbol = symbol,
                    CurrentPrice = stockPrice.CurrentPrice,
                    FridayDate = fridayDate,
                    CallOption1Percent = callOption1Percent,
                    CallOption2Percent = callOption2Percent,
                    CallOption3Percent = callOption3Percent
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating options for {symbol}: {ex.Message}");
                
                // Return partial result with error indication
                return new OptionResult
                {
                    Symbol = symbol,
                    CurrentPrice = 0,
                    FridayDate = DateTime.Today.ToString("yyyy-MM-dd"),
                    CallOption1Percent = null,
                    CallOption2Percent = null,
                    CallOption3Percent = null
                };
            }
        }
    }
}
