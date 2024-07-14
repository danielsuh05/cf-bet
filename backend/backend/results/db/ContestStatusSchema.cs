using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.results.db;

public class ContestStatusSchema
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public int ContestId { get; set; }
    public ContestStatus Status { get; set; }
}

public enum ContestStatus
{
    Incomplete,
    Closed,
    Complete,
}