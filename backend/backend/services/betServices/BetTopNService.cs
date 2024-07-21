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

            int numBetter = elos.Select(elo => MathUtils.BoxMullerTransform(elo, MathUtils.EloStd))
                .Count(curElo => curElo > curWinnerElo);

            if (numBetter < topN)
            {
                Interlocked.Increment(ref numSuccess);
            }
        });

        return Math.Min(Math.Max(0.001, (double)numSuccess / MathUtils.CountMonteCarloSimulations), 0.999);
    }

    public async Task PlaceBet(BetSchema schema)
    {
        try
        {
            decimal userBalance = await service.GetBalance(schema.Username!);
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
            await service.SetBalance(schema.Username!, userBalance - schema.InitialBet);

            await service.PutBet(schema);
        }
        catch (RestException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<BetSchema> GetBetDetails(BetSchema schema)
    {
        try
        {
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

            return schema;
        }
        catch (RestException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }
}