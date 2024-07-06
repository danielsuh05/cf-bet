using backend.interfaces;
using backend.results.betting;

namespace backend.clients;

public class TopNBetClient(string handle, int? n) : IBetClient
{
    private string _handle = handle;
    private int? _n = n;

    public Task<BetResult> PlaceBet()
    {
        throw new NotImplementedException();
    }

    public Task<BetResult> GetBetInfo()
    {
        throw new NotImplementedException();
    }
}