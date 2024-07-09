using backend.interfaces;
using backend.results.betting;
using backend.services;

namespace backend.clients;

public class OutrightWinnerBetClient(int contestId, string handle) : IBetClient
{
    private int _contestId = contestId;
    private string _handle = handle;

    public Task<int> GetMoneyLine(ContestService contestService)
    {
        throw new NotImplementedException();
    }

    public async Task<BetResult> PlaceBet(ContestService contestService, MongoDBContext context, string userId)
    {
        throw new NotImplementedException();
    }

    public Task<BetResult> GetBetInfo()
    {
        throw new NotImplementedException();
    }
}