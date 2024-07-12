using backend.results.db;
using MongoDB.Driver;

namespace backend.services;

public class MongoDBContext
{
    private readonly IMongoDatabase _database;

    public MongoDBContext(string? connectionUrl, string? databaseName)
    {
        var client = new MongoClient(connectionUrl);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<UserSchema> Users => _database.GetCollection<UserSchema>("Users");
    public IMongoCollection<ContestSchema> Contests => _database.GetCollection<ContestSchema>("Contests");

    public IMongoCollection<ContestStatusSchema> ContestStatuses =>
        _database.GetCollection<ContestStatusSchema>("ContestStatuses");

    public IMongoCollection<ContestCompetitorsSchema> ContestCompetitors =>
        _database.GetCollection<ContestCompetitorsSchema>("ContestCompetitors");
}