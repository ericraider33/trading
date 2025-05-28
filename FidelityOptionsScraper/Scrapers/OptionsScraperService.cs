using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;
using FidelityOptionsScraper.Models;
using FidelityOptionsScraper.Services;

namespace FidelityOptionsScraper.Scrapers
{
    public class OptionsScraperService
    {
        private readonly BrowserService _browserService;

        public OptionsScraperService(BrowserService browserService)
        {
            _browserService = browserService;
        }

        /// <summary>
        /// Gets call option prices for a given symbol, expiration date, and strike prices
        /// </summary>
        /// <param name="symbol">The stock symbol</param>
        /// <param name="expirationDate">The option expiration date</param>
        /// <param name="strikePrices">List of strike prices to find</param>
        /// <returns>List of OptionData objects with price information</returns>
        public async Task<List<OptionData>> GetCallOptionPricesAsync(string symbol, DateTime expirationDate, List<decimal> strikePrices)
        {
            if (_browserService.CurrentPage == null)
            {
                throw new InvalidOperationException("Browser not initialized");
            }

            var results = new List<OptionData>();

            try
            {
                // Navigate to the options chain page
                string url = $"https://digital.fidelity.com/ftgw/digital/options-research/option-chain?symbol={symbol}&oarchain=true";
                await _browserService.NavigateToAsync(url);

                // Wait for the options chain to load
                await _browserService.CurrentPage.WaitForSelectorAsync(".option-chain-table", new PageWaitForSelectorOptions { Timeout = 15000 });

                // Select the expiration date
                string formattedDate = expirationDate.ToString("MMM dd, yyyy");
                
                // Click on the expiration date dropdown
                await _browserService.CurrentPage.ClickAsync("select[data-id='expirationDate-dropdown']");
                
                // Find and select the closest expiration date
                var expirationOptions = await _browserService.CurrentPage.QuerySelectorAllAsync("select[data-id='expirationDate-dropdown'] option");
                
                bool foundExpiration = false;
                foreach (var option in expirationOptions)
                {
                    string dateText = await option.TextContentAsync() ?? "";
                    if (dateText.Contains(expirationDate.ToString("MMM")) && dateText.Contains(expirationDate.Day.ToString()))
                    {
                        await option.ClickAsync();
                        foundExpiration = true;
                        break;
                    }
                }

                if (!foundExpiration)
                {
                    // If exact date not found, select the closest available date
                    await expirationOptions[1].ClickAsync(); // Select first available date after current
                    string selectedDate = await _browserService.CurrentPage.EvaluateAsync<string>("() => document.querySelector('select[data-id=\"expirationDate-dropdown\"]').value");
                    Console.WriteLine($"Exact expiration date not found. Using: {selectedDate}");
                }

                // Wait for the options chain to update
                await _browserService.CurrentPage.WaitForTimeoutAsync(1000);

                // Process each strike price
                foreach (var targetStrike in strikePrices)
                {
                    // Find the closest strike price row
                    var optionRows = await _browserService.CurrentPage.QuerySelectorAllAsync(".option-chain-table tbody tr");
                    
                    IElementHandle? closestRow = null;
                    decimal closestDiff = decimal.MaxValue;
                    decimal actualStrike = 0;
                    
                    foreach (var row in optionRows)
                    {
                        var strikeElement = await row.QuerySelectorAsync("td.strike-price");
                        if (strikeElement != null)
                        {
                            string strikeText = await strikeElement.TextContentAsync() ?? "0";
                            strikeText = strikeText.Trim().Replace("$", "").Replace(",", "");
                            
                            if (decimal.TryParse(strikeText, out decimal strike))
                            {
                                decimal diff = Math.Abs(strike - targetStrike);
                                if (diff < closestDiff)
                                {
                                    closestDiff = diff;
                                    closestRow = row;
                                    actualStrike = strike;
                                }
                            }
                        }
                    }

                    if (closestRow != null)
                    {
                        // Extract the call option price
                        var callPriceElement = await closestRow.QuerySelectorAsync("td.call-last-price");
                        string callPriceText = await callPriceElement?.TextContentAsync() ?? "0";
                        callPriceText = callPriceText.Trim().Replace("$", "").Replace(",", "");
                        
                        decimal callPrice = 0;
                        decimal.TryParse(callPriceText, out callPrice);

                        results.Add(new OptionData
                        {
                            Symbol = symbol,
                            StrikePrice = actualStrike,
                            ExpirationDate = expirationDate,
                            CallPrice = callPrice
                        });
                    }
                    else
                    {
                        // If no matching row found, add a placeholder
                        results.Add(new OptionData
                        {
                            Symbol = symbol,
                            StrikePrice = targetStrike,
                            ExpirationDate = expirationDate,
                            CallPrice = null
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting options data for {symbol}: {ex.Message}");
                
                // Add placeholder results for all requested strike prices
                foreach (var strike in strikePrices)
                {
                    results.Add(new OptionData
                    {
                        Symbol = symbol,
                        StrikePrice = strike,
                        ExpirationDate = expirationDate,
                        CallPrice = null
                    });
                }
            }

            return results;
        }
    }
}
