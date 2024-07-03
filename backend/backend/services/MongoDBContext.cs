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

    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
}