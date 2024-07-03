using System.Net;
using backend.api;
using backend.results.codeforces;
using backend.utils;

namespace backend.services;

public class ContestService
{
    public static async Task<List<Contest>?> GetActiveContests()
    {
        try
        {
            var response = await CodeforcesApi.GetContestInfo(); 
            
            if (response == null || response.Status != "OK")
            {
                throw new RestException(HttpStatusCode.NotFound, "Failed to retrieve contest information"); 
            }

            var activeContests = response.Result?.Where(contest => contest.Phase == "BEFORE").ToList();
            return activeContests;
        }
        catch (RestException e)
        {
            Console.WriteLine($"Error: {e.Code} - {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error: {e.Message}");
            return null;
        }
    }
}