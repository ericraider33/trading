using algorithm.model;

namespace algorithm.service;

public class HistoryRepository
{
    private readonly HistoryCsv historyCsv;
    private Dictionary<string, List<History>>? historyMap;

    public HistoryRepository(HistoryCsv historyCsv)
    {
        this.historyCsv = historyCsv;
    }

    private Task<Dictionary<string, List<History>>> loadHistory()
    {
        Dictionary<string, List<History>> toLoad = new ();
        
        string[] files = Directory.GetFiles(".", "history_*.csv");
        foreach (string toRead in files)    
        {
            string symbol = toRead.Replace("history_", "").Replace(".csv", "");
            List<History> historyList = historyCsv.readCsv(toRead);
            foreach (History history in historyList)
                history.symbol = symbol;
            
            toLoad.Add(symbol, historyList);
        }

        return Task.FromResult(toLoad);
    }

    public async Task<Dictionary<string, List<History>>> findAllHistory()
    {
        if (historyMap == null)
            historyMap = await loadHistory();

        return historyMap;
    }
    
    public async Task<List<History>?> findHistory(string symbol)
    {
        if (historyMap == null)
            historyMap = await loadHistory();
        
        return historyMap.GetValueOrDefault(symbol);
    }
}