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

        return Math.Min(Math.Max(0.001, (double)numWins / MathUtils.CountMonteCarloSimulations), 0.999);
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