using backend.interfaces;
using backend.results.betting;

namespace backend.clients;

public class OutrightWinnerBetClient(string handle) : IBetClient
{
    private string _handle = handle;

    public Task<BetResult> PlaceBet()
    {
        throw new NotImplementedException();
    }

    public Task<BetResult> GetBetInfo()
    {
        throw new NotImplementedException();
    }
}