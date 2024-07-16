using backend.results.codeforces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.results.db;

public class ContestSchema
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public int ContestId { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Phase { get; set; }
    public bool Frozen { get; set; }
    public int DurationSeconds { get; set; }
    public long StartTimeSeconds { get; set; }
    public int RelativeTimeSeconds { get; set; }
}