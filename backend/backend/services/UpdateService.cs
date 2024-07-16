using backend.interfaces;
using backend.results.db;
using backend.utils;
using MongoDB.Driver;

namespace backend.services;

public class UpdateService(ICodeforcesClient codeforcesClient, MongoDbContext context, MongoDbService DbService)
{
    public async Task CheckContests()
    {
        var contestsDocument = new ContestSchema
        {
            Contests = await codeforcesClient.GetCurrentContests()
        };

        var contestIds = contestsDocument.Contests.Where(c => c != null).Select(c => c!.Id).ToList();

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

        // code above + this if statement adds new contests to ContestStatuses
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

    public async Task UpdateContests()
    {
        // get the most recently finished contest
        var contests = await codeforcesClient.GetCurrentContests();


        // TODO: REMOVE TESTING
        {
            var testFilter = Builders<ContestStatusSchema>.Filter.Where(contest => contest.ContestId == 1988);
            var update1 = Builders<ContestStatusSchema>.Update.Set(contest => contest.Status, ContestStatus.Closed);

            await context.ContestStatuses.UpdateManyAsync(testFilter, update1);
        }

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

        // incomplete contests are the contests we need to update
        // TODO: add logic for if the top person doesn't participate in the contest
        foreach (var contest in incompleteContests)
        {
            int contestId = contest.ContestId;
            Console.WriteLine(contestId);
            var rankings = await codeforcesClient.GetRankings(contestId);
            if (rankings == null)
            {
                throw new Exception("Could not get rankings from Codeforces API.");
            }

            foreach (var u in rankings)
            {
                Console.WriteLine(u.Handle);
            }

            var contestFilter = Builders<BetSchema>.Filter.Where(bet => bet.ContestId == contestId);
            var bets = await context.Bets.Find(contestFilter).ToListAsync();

            foreach (var currentBet in bets)
            {
                bool hit = false;
                bool invalid = false;

                int handleRanking;

                switch (currentBet.BetType)
                {
                    case BetType.Compete:
                        int ranking1 = rankings.FindIndex(b => b.Handle == currentBet.BetHandle1);
                        int ranking2 = rankings.FindIndex(b => b.Handle == currentBet.BetHandle2);

                        Console.WriteLine(ranking1);
                        Console.WriteLine(ranking2);

                        if (ranking1 == -1 && ranking2 == -1)
                        {
                            invalid = true;
                        }

                        if (ranking2 == -1 || ranking1 < ranking2)
                        {
                            hit = true;
                        }

                        break;
                    case BetType.Winner:
                        handleRanking = rankings.FindIndex(b => b.Handle == currentBet.WinnerBetHandle);

                        if (handleRanking == 0)
                        {
                            hit = true;
                        }

                        break;
                    case BetType.TopN:
                        handleRanking = rankings.FindIndex(b => b.Handle == currentBet.TopNBetHandle);
                        int? predictedRanking = currentBet.Ranking;

                        if (handleRanking < predictedRanking)
                        {
                            hit = true;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var betFilter = Builders<BetSchema>.Filter.Eq(b => b.Id, currentBet.Id);

                // if they both placed out of t250, don't do anything
                if (invalid)
                {
                    continue;
                }

                if (hit)
                {
                    decimal profit = currentBet.InitialBet / (decimal)currentBet.Probability!;
                    Console.WriteLine("profit: " + profit);

                    var updateBet = Builders<BetSchema>.Update.Set(b => b.ProfitLoss, profit - currentBet.InitialBet);
                    var updateHit = Builders<BetSchema>.Update.Set(b => b.Status, BetStatus.Hit);

                    await context.Bets.UpdateOneAsync(betFilter, updateBet);
                    await context.Bets.UpdateOneAsync(betFilter, updateHit);

                    decimal balance = await DbService.GetBalance(currentBet.UserId!);
                    await DbService.SetBalance(currentBet.UserId!, balance + profit);
                }
                else
                {
                    var updateBet = Builders<BetSchema>.Update.Set(b => b.ProfitLoss, -currentBet.InitialBet);
                    var updateMiss = Builders<BetSchema>.Update.Set(b => b.Status, BetStatus.Miss);
                    await context.Bets.UpdateOneAsync(betFilter, updateBet);
                    await context.Bets.UpdateOneAsync(betFilter, updateMiss);
                }
            }
        }
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
                    currentPreContest.Id);
            var update = Builders<ContestCompetitorsSchema>.Update
                .Set(contest => contest.Competitors, competitors);

            if (await context.ContestCompetitors.CountDocumentsAsync(filter) == 0)
            {
                await context.ContestCompetitors.InsertOneAsync(new ContestCompetitorsSchema
                {
                    ContestId = currentPreContest.Id,
                    Competitors = competitors
                });
            }

            await context.ContestCompetitors.UpdateManyAsync(filter, update);
        }
    }
}