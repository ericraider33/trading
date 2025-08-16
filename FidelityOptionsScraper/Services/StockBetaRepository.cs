using FidelityOptionsScraper.Models;
using FidelityOptionsScraper.Utils;

namespace FidelityOptionsScraper.Services;

public class StockBetaRepository
{
    private const string FILE_NAME = "stock_beta.csv";
    private readonly StockBetaCsv stockBetaCsv;
    private Dictionary<string, StockBeta>? betaMap;

    public StockBetaRepository(StockBetaCsv stockBetaCsv)
    {
        this.stockBetaCsv = stockBetaCsv;
    }
    
    private Task<Dictionary<string, StockBeta>> load()
    {
        Dictionary<string, StockBeta> toLoad = new ();

        if (File.Exists(FILE_NAME))
        {
            List<StockBeta> betaList = stockBetaCsv.readCsv(FILE_NAME);
            foreach (StockBeta stockBeta in betaList)
                toLoad.Add(stockBeta.symbol, stockBeta);
        }
        
        return Task.FromResult(toLoad);
    }

    public async Task<StockBeta?> findBeta(string symbol)
    {
        if (betaMap == null)
            betaMap = await load();
        
        return betaMap.GetValueOrDefault(symbol);
    }

    public async Task addBeta(StockBeta stockBeta)
    {
        if (betaMap == null)
            betaMap = await load();
        
        betaMap[stockBeta.symbol] = stockBeta;
    }

    public async Task save()
    {
        if (betaMap == null)
            betaMap = await load();

        List<StockBeta> betaList = betaMap.Values.ToList();
        stockBetaCsv.writeCsv(betaList, FILE_NAME);
    }
}