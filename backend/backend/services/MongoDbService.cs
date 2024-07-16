using backend.results.codeforces;
using backend.results.db;
using backend.utils;
using MongoDB.Driver;

namespace backend.services;

public class MongoDbService(MongoDbContext context)
{
    public async Task<List<Competitor>?> GetTopNCompetitorsFromDb(int id)
    {
        try
        {
            var filter = Builders<ContestCompetitorsSchema>.Filter.Eq(contest => contest.ContestId, id);
            var competitors = await context.ContestCompetitors.Find(filter).ToListAsync();
            if (competitors.First().Competitors == null || competitors.First().Competitors!.Count == 0)
            {
                throw new Exception("No users have signed up for this contest yet.");
            }

            return competitors.First().Competitors;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task PutBet(BetSchema bet)
    {
        try
        {
            var filter = Builders<BetSchema>.Filter.Eq(contest => contest.ContestId, bet.ContestId) &
                         Builders<BetSchema>.Filter.Eq(contest => contest.UserId, bet.UserId);
            if (await context.Bets.Find(filter).AnyAsync())
            {
                throw new Exception("Bet already placed for this contest.");
            }

            await context.Bets.InsertOneAsync(bet);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<List<BetSchema>> GetUserBets(string userId)
    {
        try
        {
            var filter = Builders<BetSchema>.Filter.Eq(user => user.UserId, userId);
            var bets = await context.Bets.Find(filter).ToListAsync();
            if (bets == null || bets.Count == 0)
            {
                throw new Exception($"Could not find any bets associated with the user ${userId}");
            }

            return bets;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}