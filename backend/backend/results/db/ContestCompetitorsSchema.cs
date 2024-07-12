using backend.results.codeforces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.results.db;

public class ContestCompetitorsSchema
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public int ContestId { get; set; }
    public List<Competitor>? Competitors { get; set; }
}