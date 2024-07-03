namespace backend.results;

public class Party
{
    public int ContestId { get; set; }
    public Member[] Members { get; set; }
    public string ParticipantType { get; set; }
    public bool Ghost { get; set; }
    public int Room { get; set; }
    public long StartTimeSeconds { get; set; }
}