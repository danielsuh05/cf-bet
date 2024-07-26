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
            var sort = Builders<BetSchema>.Sort.Descending(bet => bet.ContestId);

            var bets = await context.Bets.Find(filter).Sort(sort).ToListAsync();
            bets = bets.OrderByDescending(b => b.Date).ToList();

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
            bets = bets.OrderByDescending(b => b.Date).ToList();

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
                Id = contest.ContestId,
                Frozen = contest.Frozen,
                DurationSeconds = contest.DurationSeconds,
                StartTimeSeconds = contest.StartTimeSeconds,
                RelativeTimeSeconds = contest.RelativeTimeSeconds
            }).OrderBy(c => c.StartTimeSeconds).ToList();

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

            bets = bets.OrderByDescending(b => b.Date).ToList();

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

            var rankings = await context.Users
                .Find(filter)
                .Limit(250)
                .ToListAsync();

            rankings = rankings.OrderByDescending(u => u.MoneyBalance).ToList();

            return rankings;
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    private async Task<int> GetUserRank(string username)
    {
        try
        {
            var filter = Builders<UserSchema>.Filter.Empty;
            var rankings = await context.Users
                .Find(filter)
                .Limit(250)
                .ToListAsync();

            rankings = rankings.OrderByDescending(u => u.MoneyBalance).ToList();

            int userRank =
                rankings.FindIndex(user => user.Username == username) + 1;

            if (userRank == 0)
            {
                throw new RestException(HttpStatusCode.NotFound, "User not found in rankings.");
            }

            return userRank;
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    private async Task<int> GetNumUsers()
    {
        try
        {
            var filter = Builders<UserSchema>.Filter.Empty;

            long numUsers = await context.Users.CountDocumentsAsync(filter);

            return (int)numUsers;
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    private async Task<decimal> GetHitPercentage(string username)
    {
        try
        {
            var filter = Builders<BetSchema>.Filter.Eq(bet => bet.Username, username) &
                         Builders<BetSchema>.Filter.Eq(bet => bet.Status, BetStatus.Hit);
            var emptyFilter = Builders<BetSchema>.Filter.Eq(bet => bet.Username, username);

            int hitBets = (await context.Bets.Find(filter).ToListAsync()).Count;
            int allBets = (await context.Bets.Find(emptyFilter).ToListAsync()).Count;

            return allBets == 0 ? 0 : (decimal)hitBets / allBets;
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


    public async Task<UserProfile> GetUser(string username)
    {
        try
        {
            var ret = new UserProfile();

            var filter = Builders<UserSchema>.Filter.Eq(user => user.Username, username);

            var user = (await context.Users.Find(filter).ToListAsync()).First();

            ret.Username = username;
            ret.MoneyBalance = user.MoneyBalance;
            ret.Rank = await GetUserRank(username) + "/" + await GetNumUsers();
            ret.HitPercentage = await GetHitPercentage(username);

            return ret;
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }
}