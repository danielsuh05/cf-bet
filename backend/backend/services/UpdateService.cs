using backend.interfaces;
using backend.results.db;
using MongoDB.Driver;

namespace backend.services;

public class UpdateService(ICodeforcesClient client, MongoDBContext context)
{
    public async Task CheckContests()
    {
        var currentContests = new ContestSchema
        {
            Contests = await client.GetCurrentContests()
        };

        var dbContests = await (await context.Contests.FindAsync(Builders<ContestSchema>.Filter.Empty)).ToListAsync();

        var dbContestsIds = dbContests
            .SelectMany(c => c.Contests)
            .Select(c => c!.Id)
            .ToHashSet();

        var newContests = currentContests.Contests
            .Where(c => !dbContestsIds.Contains(c!.Id))
            .ToList();

        if (newContests.Count != 0)
        {
            var updateDefinition = Builders<ContestSchema>.Update.PushEach(c => c.Contests, newContests);

            await context.Contests.UpdateManyAsync(
                filter: Builders<ContestSchema>.Filter.Empty,
                update: updateDefinition
            );

            Console.WriteLine($"New contest found.");
        }
        else
        {
            Console.WriteLine($"No new contests to add.");
        }

        await context.Contests.InsertOneAsync(currentContests);
    }

    public async Task UpdateLeaderboard()
    {
        Console.WriteLine("not implemented");
    }

    public async Task UpdateBets()
    {
        var contests = await client.GetCurrentContests();
        var finishedContests = contests.OrderByDescending(contest => contest!.RelativeTimeSeconds)
            .First(contest => contest!.Phase == "FINISHED");

        // get the contest in the database with that id
        // if that contest is still in closed mode, then apply
    }
}