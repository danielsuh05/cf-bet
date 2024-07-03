namespace backend.results.codeforces;

public class ProblemResult
{
    public double Points { get; set; }
    public int RejectedAttemptCount { get; set; }
    public string? Type { get; set; }
    public int BestSubmissionTimeSeconds { get; set; }
}