using backend.interfaces;
using backend.results.betting;

namespace backend.clients;

public class OutrightWinnerBetClient(string handle) : IBetClient
{
    private string _handle = handle;

    public void PlaceBet()
    {
        throw new NotImplementedException();
    }

    public BetResult GetBetInfo()
    {
        throw new NotImplementedException();
    }
}