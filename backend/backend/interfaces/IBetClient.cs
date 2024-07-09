using backend.results.betting;
using backend.services;

namespace backend.interfaces;

public interface IBetClient
{
    Task<int> GetMoneyLine(ContestService contestService);

    Task<BetResult> PlaceBet(ContestService contestService, MongoDBContext context, string userId);

    Task<BetResult> GetBetInfo();
}