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

    public double MoneyBalance { get; set; }
}