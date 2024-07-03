namespace backend.results.codeforces;

public class Row
{
    public Party? Party { get; set; }
    public int Rank { get; set; }
    public double Points { get; set; }
    public int Penalty { get; set; }
    public int SuccessfulHackCount { get; set; }
    public int UnsuccessfulHackCount { get; set; }
    public ProblemResult[]? ProblemResults { get; set; }
}