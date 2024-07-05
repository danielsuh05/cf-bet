using backend.interfaces;
using backend.results.betting;

namespace backend.clients;

/// <summary>
/// Places a bet that handle1 will beat handle2.
/// </summary>
/// <param name="handle1">handle of winner</param>
/// <param name="handle2">handle of loser</param>
public class CompeteBetClient(string handle1, string handle2) : IBetClient
{
    private string _handle1 = handle1;
    private string _handle2 = handle2;

    public void PlaceBet()
    {
        throw new NotImplementedException();
    }

    public BetResult GetBetInfo()
    {
        throw new NotImplementedException();
    }
}