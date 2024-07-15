using System.Net;
using backend.results.db;
using backend.utils;

namespace backend.services.betServices;

public class BetCompeteService(MongoDbService service)
{
    private static double GetProbability(int elo1, int elo2)
    {
        return 1.0 / (1 + double.Pow(10.0, (elo2 - elo1) / 400.0));
    }

    public async Task<BetSchema> PlaceBet(BetSchema schema)
    {
        try
        {
            string handle1 = schema.BetHandle1!;
            string handle2 = schema.BetHandle2!;

            var competitors = await service.GetTopNCompetitorsFromDb(schema.ContestId);

            var competitor1 = competitors!.FirstOrDefault(c => c.Handle == handle1);
            var competitor2 = competitors!.FirstOrDefault(c => c.Handle == handle2);

            if (competitor1 == null)
            {
                throw new RestException(HttpStatusCode.NotFound, $"Could not find user {handle1}");
            }

            if (competitor2 == null)
            {
                throw new RestException(HttpStatusCode.NotFound, $"Could not find user {handle2}");
            }

            double probability = GetProbability(competitor1!.Ranking, competitor2!.Ranking);
            schema.Probability = probability;
            schema.Status = BetStatus.Pending;

            return schema;
        }
        catch (RestException e)
        {
            throw new Exception(e.ErrorMessage);
        }
    }
}