using Newtonsoft.Json;
using OptionsPriceFinder.Models;
using System.Net.Http;

namespace OptionsPriceFinder.Services
{
    public class PolygonApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public PolygonApiClient(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
            _httpClient.BaseAddress = new Uri("https://api.polygon.io/");
        }

        public async Task<StockPrice> GetCurrentStockPriceAsync(string symbol)
        {
            try
            {
                var response = await _httpClient.GetAsync($"v2/last/trade/{symbol}?apiKey={_apiKey}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PolygonTradeResponse>(content);

                if (result?.Status == "OK" && result.Results != null)
                {
                    return new StockPrice
                    {
                        Symbol = symbol,
                        CurrentPrice = result.Results.Price,
                        RetrievalTime = DateTimeOffset.FromUnixTimeMilliseconds(result.Results.Timestamp / 1000000).DateTime
                    };
                }

                throw new Exception($"Failed to get price for {symbol}: {content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching stock price for {symbol}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<OptionData>> GetOptionsDataAsync(string symbol, DateTime expirationDate, decimal targetPrice)
        {
            try
            {
                // Format the expiration date as YYYY-MM-DD
                string formattedDate = expirationDate.ToString("yyyy-MM-dd");
                
                // Get options contracts for the given symbol and expiration date
                var response = await _httpClient.GetAsync(
                    $"v3/reference/options/contracts?underlying_ticker={symbol}&expiration_date={formattedDate}&apiKey={_apiKey}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PolygonOptionsResponse>(content);

                if (result?.Results == null || !result.Results.Any())
                {
                    Console.WriteLine($"No options found for {symbol} with expiration date {formattedDate}");
                    return new List<OptionData>();
                }

                // Filter for call options with strike prices close to target price
                var callOptions = result.Results
                    .Where(o => o.ContractType == "call")
                    .OrderBy(o => Math.Abs(o.StrikePrice - targetPrice))
                    .Take(5) // Take a few closest matches
                    .ToList();

                var optionsData = new List<OptionData>();
                
                // For each contract, get the latest price
                foreach (var contract in callOptions)
                {
                    try
                    {
                        var priceResponse = await _httpClient.GetAsync(
                            $"v2/last/trade/O:{contract.Ticker}?apiKey={_apiKey}");
                        
                        if (priceResponse.IsSuccessStatusCode)
                        {
                            var priceContent = await priceResponse.Content.ReadAsStringAsync();
                            var priceResult = JsonConvert.DeserializeObject<PolygonTradeResponse>(priceContent);

                            if (priceResult?.Status == "OK" && priceResult.Results != null)
                            {
                                optionsData.Add(new OptionData
                                {
                                    Symbol = symbol,
                                    StrikePrice = contract.StrikePrice,
                                    ExpirationDate = DateTime.Parse(contract.ExpirationDate),
                                    CallPrice = priceResult.Results.Price
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error fetching option price for {contract.Ticker}: {ex.Message}");
                    }
                }

                return optionsData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching options data for {symbol}: {ex.Message}");
                throw;
            }
        }
    }

    // Response models for Polygon API
    public class PolygonTradeResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("results")]
        public TradeResult Results { get; set; }
    }

    public class TradeResult
    {
        [JsonProperty("p")]
        public decimal Price { get; set; }

        [JsonProperty("t")]
        public long Timestamp { get; set; }
    }

    public class PolygonOptionsResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("results")]
        public List<OptionContract> Results { get; set; }
    }

    public class OptionContract
    {
        [JsonProperty("ticker")]
        public string Ticker { get; set; }

        [JsonProperty("underlying_ticker")]
        public string UnderlyingTicker { get; set; }

        [JsonProperty("expiration_date")]
        public string ExpirationDate { get; set; }

        [JsonProperty("strike_price")]
        public decimal StrikePrice { get; set; }

        [JsonProperty("contract_type")]
        public string ContractType { get; set; }
    }
}
