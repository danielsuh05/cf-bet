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
            var filter = Builders<BetSchema>.Filter.Eq(bet => bet.UserId, userId);
            var bets = await context.Bets.Find(filter).ToListAsync();

            return bets;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<List<BetSchema>> GetUserContestBets(string userId, int contestId)
    {
        try
        {
            var filter = Builders<BetSchema>.Filter.Eq(user => user.UserId, userId) &
                         Builders<BetSchema>.Filter.Eq(bet => bet.ContestId, contestId);
            var bets = await context.Bets.Find(filter).ToListAsync();

            return bets;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<decimal> GetBalance(string userId)
    {
        try
        {
            var filter = Builders<UserSchema>.Filter.Eq(user => user.Id, userId);
            return (await context.Users.Find(filter).ToListAsync()).First().MoneyBalance;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task SetBalance(string userId, decimal balance)
    {
        try
        {
            var filter = Builders<UserSchema>.Filter.Eq(user => user.Id, userId);
            var update = Builders<UserSchema>.Update.Set(user => user.MoneyBalance, balance);

            await context.Users.UpdateOneAsync(filter, update);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<List<Contest>> GetCurrentContests()
    {
        try
        {
            var filter = Builders<ContestSchema>.Filter.Eq(contest => contest.Phase, "BEFORE");
            var contests = await context.Contests.Find(filter).ToListAsync();
            var retContests = contests.Select(contest => new Contest
            {
                Name = contest.Name,
                Type = contest.Type,
                Phase = contest.Phase,
                Frozen = contest.Frozen,
                DurationSeconds = contest.DurationSeconds,
                StartTimeSeconds = contest.StartTimeSeconds,
                RelativeTimeSeconds = contest.RelativeTimeSeconds
            }).ToList();

            return retContests;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<List<BetSchema>> GetContestBets(int id)
    {
        try
        {
            var filter = Builders<BetSchema>.Filter.Eq(bet => bet.ContestId, id);

            var bets = await context.Bets.Find(filter).ToListAsync();

            return bets;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<List<UserSchema>> GetRankings()
    {
        try
        {
            var filter = Builders<UserSchema>.Filter.Empty;
            var sort = Builders<UserSchema>.Sort.Descending(u => u.MoneyBalance);

            var rankings = await context.Users
                .Find(filter)
                .Sort(sort)
                .Limit(250)
                .ToListAsync();

            return rankings;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}