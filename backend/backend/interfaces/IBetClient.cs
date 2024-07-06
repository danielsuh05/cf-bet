using backend.results.betting;

namespace backend.interfaces;

public interface IBetClient
{
    Task<BetResult> PlaceBet();

    Task<BetResult> GetBetInfo();
}