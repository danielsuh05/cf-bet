using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.results.betting;

public class BetEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string? UserId { get; set; }
    public int ContestId { get; set; }

    public BetType BetType { get; set; }
    public BetStatus Status { get; set; }
    public decimal InitialBet { get; set; }
    public decimal? ProfitLoss { get; set; }
    public double? Probability { get; set; }

    // outright winner
    public string? WinnerBetHandle { get; set; }

    // top n
    public string? TopNBetHandle { get; set; }
    public int? Ranking { get; set; }

    // compete bet request
    public string? BetHandle1 { get; set; }
    public string? BetHandle2 { get; set; }
}

public enum BetType
{
    Compete,
    TopN,
    Winner
}

public enum BetStatus
{
    Hit,
    Miss,
    Pending,
}