using backend.interfaces;
using backend.results;
using backend.results.codeforces;
using backend.utils;
using Newtonsoft.Json;

namespace backend.api;

public class CodeforcesClient(HttpClient client) : ICodeforcesClient
{
    public async Task<UserResultResponse?> GetUserInfo(string username)
    {
        try
        {
            var apiUrl = $"https://codeforces.com/api/user.info?handles={username}";
            var response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonConvert.DeserializeObject<UserResultResponse>(responseBody);
            return apiResponse;
        }
        catch (HttpRequestException e)
        {
            throw new RestException(e.StatusCode, e.Message);
        }
    }

    public async Task<ContestListInfo?> GetContestInfo()
    {
        try
        {
            const string apiUrl = $"https://codeforces.com/api/contest.list?gym=false";
            var response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonConvert.DeserializeObject<ContestListInfo>(responseBody);
            return apiResponse;
        }
        catch (HttpRequestException e)
        {
            throw new RestException(e.StatusCode, e.Message);
        }
    }
}