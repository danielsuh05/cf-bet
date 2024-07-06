using backend.results.betting;

namespace backend.results.requests;

public class BetRequest
{
    public BetType BetType { get; set; }
    public string? Username { get; set; }
    public decimal InitialBet { get; set; }

    // compete bet request
    public string? BetHandle1 { get; set; }
    public string? BetHandle2 { get; set; }

    // top n bet request
    public string? TopNBetHandle { get; set; }
    public int? Ranking { get; set; }

    // outright winner bet request
    public string? WinnerBetHandle { get; set; }
}