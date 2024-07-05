using backend.interfaces;
using backend.results.betting;

namespace backend.clients;

public class TopNBetClient(string handle, int n) : IBetClient
{
    private string _handle = handle;
    private int _n = n;

    public void PlaceBet()
    {
        throw new NotImplementedException();
    }

    public BetResult GetBetInfo()
    {
        throw new NotImplementedException();
    }
}