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

    public async Task UpdateContests()
    {
        // get the most recently finished contest
        var contests = await codeforcesClient.GetCurrentContests();

        var currentContests = contests.Where(contest =>
                contest!.Phase == "CODING" || contest.Phase == "PENDING_SYSTEM_TEST" || contest.Phase == "SYSTEM_TEST")
            .Select(contest => contest!.Id)
            .ToHashSet();

        var finishedContests = contests.Where(contest => contest!.Phase == "FINISHED")
            .Select(contest => contest!.Id)
            .ToHashSet();

        var currentContestsFilter =
            Builders<ContestStatusSchema>.Filter.Where(contest => currentContests.Contains(contest.ContestId));

        var update = Builders<ContestStatusSchema>.Update
            .Set(contest => contest.Status, ContestStatus.Closed);
        await context.ContestStatuses.UpdateManyAsync(currentContestsFilter, update);

        // finding the contests in contest statuses which are finished on codeforces but have not yet been processed.
        // we store these in incompleteContests and then update the contest statuses in mongodb to say that they are fully complete contests.
        // this is why we do not want the contest status to be complete when we check for them in the filter.

        var finishedContestsFilter =
            Builders<ContestStatusSchema>.Filter.Where(contest =>
                finishedContests.Contains(contest.ContestId)) &
            Builders<ContestStatusSchema>.Filter.Where(contest => contest.Status != ContestStatus.Complete);
        var incompleteContests =
            await (await context.ContestStatuses.FindAsync(finishedContestsFilter)).ToListAsync();

        // complete the contests because we are about to update them
        update = Builders<ContestStatusSchema>.Update
            .Set(contest => contest.Status, ContestStatus.Complete);
        await context.ContestStatuses.UpdateManyAsync(finishedContestsFilter, update);

        // incompletecontests are the contests we need to update
        // TODO: add logic for if the top person doesn't participate in the contest
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

            if (await context.ContestCompetitors.CountDocumentsAsync(filter) == 0)
            {
                await context.ContestCompetitors.InsertOneAsync(new ContestCompetitorsSchema
                {
                    ContestId = currentPreContest!.Id,
                    Competitors = competitors
                });
            }

            await context.ContestCompetitors.UpdateOneAsync(filter, update);
        }
    }
}