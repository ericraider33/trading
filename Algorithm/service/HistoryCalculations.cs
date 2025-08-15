using algorithm.model;

namespace algorithm.service;

public class HistoryCalculations
{
    public void calculateMovingAverages(List<History>  histories)
    {
        Queue<History> histories21 = new();
        Queue<History> histories49 = new();
        Queue<History> histories203 = new();
        
        foreach (History h in histories)
        {
            queueMovingAverage(histories21, h, 21/7);
            queueMovingAverage(histories49, h, 49/7);
            queueMovingAverage(histories203, h, 203/7);
            
            if (histories21.Count >= 21/7)
                h.movingAverage21 = calculateAverage(histories21);
            if (histories49.Count >= 49/7)
                h.movingAverage49 = calculateAverage(histories49);
            if (histories203.Count >= 203/7)
                h.movingAverage203 = calculateAverage(histories203);
        }
    }
    
    private decimal? calculateAverage(Queue<History> queue)
    {
        if (queue.Count == 0)
            return null;

        decimal sum = 0;
        foreach (History h in queue)
        {
            sum += h.open;
        }
        return sum / queue.Count;
    }
    
    private void queueMovingAverage(Queue<History> queue, History history, int size)
    {
        if (queue.Count >= size)
            queue.Dequeue();
        queue.Enqueue(history);
    }
}