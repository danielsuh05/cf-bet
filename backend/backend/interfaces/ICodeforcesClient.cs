using backend.results.codeforces;

namespace backend.interfaces;

public interface ICodeforcesClient
{
    public Task<UserResult?> GetUserInfo(string username);

    public Task<List<Contest?>> GetCurrentContests();

    public Task<List<Competitor>?> GetTopNCompetitors(int id);

    public Task<List<Member>?> GetRankings(int id);
}