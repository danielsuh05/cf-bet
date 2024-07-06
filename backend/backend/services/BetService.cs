using backend.clients;
using backend.interfaces;
using backend.results.betting;
using backend.results.requests;

namespace backend.services;

public class BetService(BetRequest request)
{
    private readonly IBetClient _client = request.BetType switch
    {
        BetType.TopN => new TopNBetClient(request.TopNBetHandle!, request.Ranking),
        BetType.OutrightWinner => new OutrightWinnerBetClient(request.WinnerBetHandle!),
        BetType.Compete => new CompeteBetClient(request.BetHandle1!, request.BetHandle2!),
        _ => throw new Exception("Unknown bet type")
    };

    public async Task<BetResult> PlaceBet()
    {
        var result = await _client.PlaceBet();
        return result;
    }

    public async Task<BetResult> GetBetInfo()
    {
        var result = await _client.GetBetInfo();
        return result;
    }
}