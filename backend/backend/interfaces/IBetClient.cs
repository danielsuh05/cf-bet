using backend.results.betting;

namespace backend.interfaces;

public interface IBetClient
{
    void PlaceBet();

    BetResult GetBetInfo(string id);
}