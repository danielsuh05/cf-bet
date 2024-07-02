using backend.results;
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
            Console.WriteLine("\nException Caught");
            Console.WriteLine($"Message: {e.Message}");
            return null;
        }
    }

    public static async Task<ContestInfo?> GetContestInfo(int id)
    {
        try
        {
            var apiUrl =
                $"https://codeforces.com/api/contest.standings?contestId={id}&asManager=false&from=1&count=200&showUnofficial=true";
            var response = await Client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonConvert.DeserializeObject<ContestInfo>(responseBody);
            return apiResponse;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught");
            Console.WriteLine($"Message: {e.Message} ");
            return null;
        }
    }
}