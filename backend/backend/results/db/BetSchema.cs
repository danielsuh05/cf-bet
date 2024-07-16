using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.results.db;

public class BetSchema
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } // set by mongodb

    public string? UserId { get; set; } // set by authorization
    public string? Username { get; set; } // set by authorization
    public int ContestId { get; set; }

    public BetType BetType { get; set; } // set by frontend
    public decimal InitialBet { get; set; } // set by frontend
    public BetStatus Status { get; set; }
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
    Invalid,
}