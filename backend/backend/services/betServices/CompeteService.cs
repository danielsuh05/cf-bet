using backend.interfaces;
using backend.results.db;
using Microsoft.AspNetCore.Http.HttpResults;

namespace backend.services.betServices;

public class CompeteService(ICodeforcesClient codeforcesClient, MongoDBContext context, JwtService jwtService)
{
    private static double GetProbability(int elo1, int elo2)
    {
        return 1.0 / (1 + double.Pow(10.0, (elo2 - elo1) / 400.0));
    }

    public async Task<BetSchema> PlaceBet(BetSchema schema)
    {
        string handle1 = schema.BetHandle1!;
        string handle2 = schema.BetHandle2!;

        var competitors = await codeforcesClient.GetTopNCompetitorsFromDb(schema.ContestId);

        foreach (var c in competitors)
        {
            Console.WriteLine(c.Handle);
        }

        return schema;

        //
        // var competitor1 = competitors!.FirstOrDefault(c => c.Handle == handle1);
        // var competitor2 = competitors!.FirstOrDefault(c => c.Handle == handle2);
        //
        // double probability = GetProbability(competitor1!.Ranking, competitor2!.Ranking);
        // schema.Probability = probability;
        //
        // return schema;
    }
}