namespace backend.results.betting;

public class BetResult(int initialBet, bool hit, int profitLoss, BetType betType, string betString)
{
    public int InitialBet = initialBet;
    public bool Hit = hit;
    public int ProfitLoss = profitLoss;
    public BetType BetType = betType;
    public string BetString = betString;
}