using backend.clients;
using backend.interfaces;
using backend.results.betting;
using backend.results.requests;

namespace backend.services;

public class BetService(ContestService contestService, MongoDBContext dbContext, BetRequest request)
{
    private readonly IBetClient _client = request.BetType switch
    {
        BetType.TopN => new TopNBetClient(request.contestId, request.TopNBetHandle!, request.Ranking),
        BetType.OutrightWinner => new OutrightWinnerBetClient(request.contestId, request.WinnerBetHandle!),
        BetType.Compete => new CompeteBetClient(request.contestId, request.BetHandle1!, request.BetHandle2!),
        _ => throw new Exception("Unknown bet type")
    };

    public async Task<int> GetMoneyLine()
    {
        int result = await _client.GetMoneyLine(contestService);
        return result;
    }

    public async Task<BetResult> PlaceBet()
    {
        var result = await _client.PlaceBet(contestService, dbContext, request.UserId!);
        return result;
    }

    public async Task<BetResult> GetBetInfo()
    {
        throw new NotImplementedException();
    }
}