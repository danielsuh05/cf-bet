using backend.results.codeforces;

namespace backend.interfaces;

public interface ICodeforcesClient
{
    public Task<UserResultResponse?> GetUserInfo(string username);

    public Task<ContestListInfo?> GetContestInfo();
}