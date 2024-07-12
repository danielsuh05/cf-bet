using backend.interfaces;
using backend.results.db;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace backend.services;

public class UpdateService(ICodeforcesClient client, MongoDBContext context)
{
    public async Task CheckContests()
    {
        var contestsDocument = new ContestSchema
        {
            Contests = await client.GetCurrentContests()
        };

        var contestIds = contestsDocument.Contests.Where(c => c != null).Select(c => c.ContestId).ToList();

        var statusFilter = Builders<ContestStatusSchema>.Filter.In(cs => cs.ContestId, contestIds);
        var existingContestStatuses = await context.ContestStatuses.Find(statusFilter).ToListAsync();
        var existingContestIds = existingContestStatuses.Select(cs => cs.ContestId).ToHashSet();

        var newContestStatusDocuments = contestsDocument.Contests
            .Where(contest => contest != null && !existingContestIds.Contains(contest.ContestId))
            .Select(contest => new ContestStatusSchema
            {
                ContestId = contest!.ContestId,
                Status = ContestStatus.Incomplete
            }).ToList();

        if (newContestStatusDocuments.Count > 0)
        {
            await context.ContestStatuses.InsertManyAsync(newContestStatusDocuments);
        }

        var filter = Builders<ContestSchema>.Filter.Empty;
        if (await context.Contests.CountDocumentsAsync(filter) == 0)
        {
            await context.Contests.InsertOneAsync(contestsDocument);
            return;
        }

        var update = Builders<ContestSchema>.Update
            .Set(contest => contest, contestsDocument);
        await context.Contests.UpdateOneAsync(filter, update);
    }

    public async Task UpdateLeaderboard()
    {
        Console.WriteLine("not implemented");
    }

    public async Task UpdateBets()
    {
        var contests = await client.GetCurrentContests();
        var finishedContest = contests.OrderByDescending(contest => contest!.RelativeTimeSeconds)
            .First(contest => contest!.Phase == "FINISHED");

        var filter = Builders<ContestStatusSchema>.Filter.Eq(contest => contest.ContestId, finishedContest!.ContestId);
        var contestStatus = await (await context.ContestStatuses.FindAsync(filter)).ToListAsync();

        if (contestStatus != null && contestStatus[0].Status == ContestStatus.Complete)
        {
            return;
        }

        var update = Builders<ContestStatusSchema>.Update
            .Set(contest => contest.Status, ContestStatus.Complete);
        await context.ContestStatuses.UpdateOneAsync(filter, update);

        // finishedContest.ContestId 
    }
}