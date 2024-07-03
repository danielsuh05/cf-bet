using backend.results;
using backend.results.codeforces;
using backend.utils;
using Newtonsoft.Json;

namespace backend.api;

public class CodeforcesApi
{
    private static readonly HttpClient Client = new HttpClient(); 
    
    public static async Task<UserResultResponse?> GetUserInfo(string username)
    {
        try
        {
            var apiUrl = $"https://codeforces.com/api/user.info?handles={username}";
            var response = await Client.GetAsync(apiUrl);
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

    public static async Task<ContestListInfo?> GetContestInfo()
    {
        try
        {
            var apiUrl =
                $"https://codeforces.com/api/contest.list?gym=false";
            var response = await Client.GetAsync(apiUrl);
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