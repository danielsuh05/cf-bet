using System.Net;
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
                throw new RestException(HttpStatusCode.NotFound, "No users have signed up for this contest yet.");
            }

            return competitors.First().Competitors;
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

    public async Task PutBet(BetSchema bet)
    {
        try
        {
            var filter = Builders<BetSchema>.Filter.Eq(contest => contest.ContestId, bet.ContestId) &
                         Builders<BetSchema>.Filter.Eq(contest => contest.Username, bet.Username);

            // bet already placed
            if (await context.Bets.Find(filter).AnyAsync())
            {
                throw new RestException(HttpStatusCode.BadRequest, "Bet already placed for this contest.");
            }

            await context.Bets.InsertOneAsync(bet);
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

    public async Task<ContestStatus> GetContestStatus(int id)
    {
        try
        {
            var filter = Builders<ContestStatusSchema>.Filter.Eq(contest => contest.ContestId, id);
            var contestStatus = await context.ContestStatuses.Find(filter).ToListAsync();
            if (contestStatus == null || contestStatus.Count == 0)
            {
                throw new RestException(HttpStatusCode.NotFound, "No users have signed up for this contest yet.");
            }

            return contestStatus.First().Status;
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

    public async Task<List<BetSchema>> GetUserBets(string username)
    {
        try
        {
            var filter = Builders<BetSchema>.Filter.Eq(bet => bet.Username, username);
            var bets = await context.Bets.Find(filter).ToListAsync();

            return bets;
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

    public async Task<List<BetSchema>> GetUserContestBets(string username, int contestId)
    {
        try
        {
            var filter = Builders<BetSchema>.Filter.Eq(user => user.Username, username) &
                         Builders<BetSchema>.Filter.Eq(bet => bet.ContestId, contestId);
            var bets = await context.Bets.Find(filter).ToListAsync();

            return bets;
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<decimal> GetBalance(string username)
    {
        try
        {
            var filter = Builders<UserSchema>.Filter.Eq(user => user.Username, username);
            return (await context.Users.Find(filter).ToListAsync()).First().MoneyBalance;
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task SetBalance(string username, decimal balance)
    {
        try
        {
            var filter = Builders<UserSchema>.Filter.Eq(user => user.Username, username);
            var update = Builders<UserSchema>.Update.Set(user => user.MoneyBalance, balance);

            await context.Users.UpdateOneAsync(filter, update);
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
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
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
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
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<List<UserSchema>> GetRankings()
    {
        try
        {
            var filter = Builders<UserSchema>.Filter.Empty;
            var sort = Builders<UserSchema>.Sort.Ascending(u => u.MoneyBalance);

            var rankings = await context.Users
                .Find(filter)
                .Sort(sort)
                .Limit(250)
                .ToListAsync();

            return rankings;
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }
}