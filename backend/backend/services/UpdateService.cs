using backend.interfaces;
using backend.results.codeforces;
using backend.results.db;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace backend.services;

public class UpdateService(ICodeforcesClient codeforcesClient, MongoDBContext context)
{
    public async Task CheckContests()
    {
        var contestsDocument = new ContestSchema
        {
            Contests = await codeforcesClient.GetCurrentContests()
        };

        var contestIds = contestsDocument.Contests.Where(c => c != null).Select(c => c.Id).ToList();

        var statusFilter = Builders<ContestStatusSchema>.Filter.In(cs => cs.ContestId, contestIds);
        var existingContestStatuses = await context.ContestStatuses.Find(statusFilter).ToListAsync();
        var existingContestIds = existingContestStatuses.Select(cs => cs.ContestId).ToHashSet();

        var newContestStatusDocuments = contestsDocument.Contests
            .Where(contest => contest != null && !existingContestIds.Contains(contest.Id))
            .Select(contest => new ContestStatusSchema
            {
                ContestId = contest!.Id,
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
        // get the most recently finished contest
        var contests = await codeforcesClient.GetCurrentContests();
        var finishedContest = contests.First(contest => contest!.Phase == "FINISHED");

        Console.WriteLine(finishedContest.Phase);

        var filter = Builders<ContestStatusSchema>.Filter.Eq(contest => contest.ContestId, finishedContest!.Id);
        var contestStatus = await (await context.ContestStatuses.FindAsync(filter)).ToListAsync();

        Console.WriteLine(contestStatus[0].ContestId);

        if (contestStatus[0].Status == ContestStatus.Complete)
        {
            return;
        }

        var update = Builders<ContestStatusSchema>.Update
            .Set(contest => contest.Status, ContestStatus.Complete);
        await context.ContestStatuses.UpdateOneAsync(filter, update);

        // finishedContest.ContestId is the id of the contest that we want ot update the bets for. 
    }

    public async Task GetCompetitors()
    {
        var contests = await codeforcesClient.GetCurrentContests();
        var beforeContests = contests.Where(contest => contest!.Phase == "BEFORE");

        foreach (var currentPreContest in beforeContests)
        {
            var competitors = await codeforcesClient.GetTopNCompetitors(currentPreContest!.Id);

            var filter =
                Builders<ContestCompetitorsSchema>.Filter.Eq(contest => contest.ContestId,
                    currentPreContest!.Id);
            var update = Builders<ContestCompetitorsSchema>.Update
                .Set(contest => contest.Competitors, competitors);

            await context.ContestCompetitors.UpdateOneAsync(filter, update);
        }
    }
}