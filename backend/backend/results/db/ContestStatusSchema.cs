using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.results.db;

public class ContestStatusSchema
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public int contestId { get; set; }
    public ContestStatus status { get; set; }
}

public enum ContestStatus
{
    Before,
    InCompetition,
    Complete,
}