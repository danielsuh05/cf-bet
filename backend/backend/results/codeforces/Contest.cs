namespace backend.results.codeforces;

public class Contest
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Phase { get; set; }
    public bool Frozen { get; set; }
    public int DurationSeconds { get; set; }
    public long StartTimeSeconds { get; set; }
    public int RelativeTimeSeconds { get; set; }
}