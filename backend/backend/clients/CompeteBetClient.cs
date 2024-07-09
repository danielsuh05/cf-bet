using backend.interfaces;
using backend.results.betting;
using backend.results.db;
using backend.services;
using backend.utils;
using MongoDB.Driver;

namespace backend.clients;

/// <summary>
/// Places a bet that handle1 will beat handle2.
/// </summary>
/// <param name="handle1">handle of winner</param>
/// <param name="handle2">handle of loser</param>
public class CompeteBetClient(int contestId, string handle1, string handle2) : IBetClient
{
    private double GetProbabilityP1Wins(double elo1, double elo2)
    {
        return 1.0 / (1.0 + Math.Pow(10.0, (elo2 - elo1) / 400.0));
    }

    public async Task<int> GetMoneyLine(ContestService contestService)
    {
        var competitors = await contestService.GetTopCompetitors(contestId);

        var comp1 = competitors!.First(x => x.Handle == handle1);
        var comp2 = competitors!.First(x => x.Handle == handle2);

        double probability = GetProbabilityP1Wins(comp1.Ranking, comp2.Ranking);

        return OddsConverter.GetAmericanOddsFromProbability(probability);
    }

    public async Task<BetResult> PlaceBet(ContestService contestService, MongoDBContext context, string userId)
    {
        var competitors = await contestService.GetTopCompetitors(contestId);

        var comp1 = competitors!.First(x => x.Handle == handle1);
        var comp2 = competitors!.First(x => x.Handle == handle2);

        double probability = GetProbabilityP1Wins(comp1.Ranking, comp2.Ranking);

        var filter = Builders<UserSchema>.Filter
            .Eq(user => user.Id, userId);

        var update = Builders<UserSchema>.Update.Push<BetResult>(u => u.Results, result);

        await context.Users.FindOneAndUpdateAsync(filter, update);
    }

    public Task<BetResult> GetBetInfo()
    {
        throw new NotImplementedException();
    }
}