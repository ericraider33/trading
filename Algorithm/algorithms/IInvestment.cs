using algorithm.model;

namespace algorithm.algorithms;

public interface IInvestment
{
    PositionInvestment run(PositionInvestment initialInvestment, List<History> historyList);
}