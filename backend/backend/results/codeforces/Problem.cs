namespace backend.results.codeforces;

public class Problem
{
    public int ContestId { get; set; }
    public string? Index { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public double Points { get; set; }
    public string[]? Tags { get; set; }
}