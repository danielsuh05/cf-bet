using backend.results.betting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.results.db;

public class UserSchema
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string? Username { get; set; }
    public string? PasswordHash { get; set; }

    public List<BetResult>? Results { get; set; }
    public double MoneyBalance { get; set; }
}