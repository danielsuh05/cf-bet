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

        var filter = Builders<ContestSchema>.Filter.Empty;
        if (await context.Contests.CountDocumentsAsync(filter) == 0)
        {
            await context.Contests.InsertOneAsync(currentContests);
            return;
        }

        var update = Builders<ContestSchema>.Update
            .Set(contest => contest, currentContests);
        await context.Contests.UpdateOneAsync(filter, update);
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