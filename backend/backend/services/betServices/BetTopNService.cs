using System.Net;
using backend.results.db;
using backend.utils;

namespace backend.services.betServices;

public class BetTopNService(MongoDbService service)
{
    private static double GetProbability(int winnerElo, int? topN, List<int> elos)
    {
        var numSuccess = 0;
        Parallel.For(0, MathUtils.CountMonteCarloSimulations, _ =>
        {
            double curWinnerElo = MathUtils.BoxMullerTransform(winnerElo, MathUtils.EloStd);
            var numBetter = 0;
            if (elos.Select(elo => MathUtils.BoxMullerTransform(elo, MathUtils.EloStd))
                .Any(compElo => compElo > curWinnerElo))
            {
                numBetter++;
            }

            if (numBetter < topN)
            {
                Interlocked.Increment(ref numSuccess);
            }
        });

        return (double)numSuccess / MathUtils.CountMonteCarloSimulations;
    }

    public async Task PlaceBet(BetSchema schema)
    {
        try
        {
            decimal userBalance = await service.GetBalance(schema.UserId!);
            if (userBalance - schema.InitialBet < 0)
            {
                throw new RestException(HttpStatusCode.Forbidden, $"Insufficient balance");
            }

            string handle = schema.TopNBetHandle!;
            int? topN = schema.Ranking!;

            var competitors = await service.GetTopNCompetitorsFromDb(schema.ContestId);

            var competitor = competitors!.FirstOrDefault(c => c.Handle == handle);

            if (competitor == null)
            {
                throw new RestException(HttpStatusCode.NotFound, $"Could not find user {handle}");
            }

            double probability = GetProbability(competitor.Ranking, topN, competitors!.Select(c => c.Ranking).ToList());

            schema.Probability = probability;
            schema.Status = BetStatus.Pending;
            await service.SetBalance(schema.UserId!, userBalance - schema.InitialBet);

            await service.PutBet(schema);
        }
        catch (RestException e)
        {
            throw new Exception(e.ErrorMessage);
        }
    }
}