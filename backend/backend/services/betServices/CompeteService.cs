using backend.interfaces;
using backend.results.betting;
using Microsoft.AspNetCore.Http.HttpResults;

namespace backend.services.betServices;

public class CompeteService(ICodeforcesClient codeforcesClient, MongoDBContext context, JwtService jwtService)
{
    private static double GetProbability(int elo1, int elo2)
    {
        return 1.0 / (1 + double.Pow(10.0, (elo2 - elo1) / 400.0));
    }

    public async Task<BetEntry> PlaceBet(BetEntry entry)
    {
        string handle1 = entry.BetHandle1!;
        string handle2 = entry.BetHandle2!;

        var competitors = await codeforcesClient.GetTopNCompetitors(entry.ContestId);

        var competitor1 = competitors!.FirstOrDefault(c => c.Handle == handle1);
        var competitor2 = competitors!.FirstOrDefault(c => c.Handle == handle2);

        double probability = GetProbability(competitor1!.Ranking, competitor2!.Ranking);
        entry.Probability = probability;

        return entry;
    }
}