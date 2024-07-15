using System.Net;
using backend.results.db;
using backend.utils;

namespace backend.services.betServices;

public class BetWinnerService(MongoDbService service)
{
    private static double GetProbability(int winnerElo, List<int> elos)
    {
        var numWins = 0;
        Parallel.For(0, MathUtils.CountMonteCarloSimulations, _ =>
        {
            double curWinnerElo = MathUtils.BoxMullerTransform(winnerElo, MathUtils.EloStd);
            if (elos.Select(elo => MathUtils.BoxMullerTransform(elo, MathUtils.EloStd))
                .Any(compElo => compElo > curWinnerElo))
            {
                return;
            }

            Interlocked.Increment(ref numWins);
        });

        return (double)numWins / MathUtils.CountMonteCarloSimulations;
    }

    public async Task<BetSchema> PlaceBet(BetSchema schema)
    {
        try
        {
            string handle = schema.WinnerBetHandle!;

            var competitors = await service.GetTopNCompetitorsFromDb(schema.ContestId);

            var competitor = competitors!.FirstOrDefault(c => c.Handle == handle);

            if (competitor == null)
            {
                throw new RestException(HttpStatusCode.NotFound, $"Could not find user {handle}");
            }

            double probability = GetProbability(competitor.Ranking, competitors!.Select(c => c.Ranking).ToList());
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