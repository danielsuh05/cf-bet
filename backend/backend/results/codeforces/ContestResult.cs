namespace backend.results;

public class ContestResult
{
    public Contest Contest { get; set; }
    public Problem[] Problems { get; set; }
    public Row[] Rows { get; set; }
}