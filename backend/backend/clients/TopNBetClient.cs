using backend.interfaces;
using backend.results.betting;
using backend.services;

namespace backend.clients;

public class TopNBetClient(int contestId, string handle, int? n) : IBetClient
{
    private int _contestId = contestId;
    private string _handle = handle;
    private int? _n = n;

    public Task<int> GetMoneyLine(ContestService contestService)
    {
        throw new NotImplementedException();
    }

    public Task<BetResult> PlaceBet(ContestService contestService, MongoDBContext context, string userId)
    {
        Console.WriteLine(userId);
        throw new NotImplementedException();
    }

    public Task<BetResult> GetBetInfo()
    {
        throw new NotImplementedException();
    }
}