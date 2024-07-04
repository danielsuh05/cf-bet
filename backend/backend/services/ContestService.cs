using System.Net;
using backend.interfaces;
using backend.results.codeforces;
using backend.utils;

namespace backend.services;

public class ContestService(ICodeforcesClient codeforcesClient)
{
    public async Task<List<Contest>?> GetActiveContests()
    {
        try
        {
            var response = await codeforcesClient.GetCurrentContests();

            var activeContests = response.Where(contest => contest!.Phase == "BEFORE").ToList();
            return activeContests!;
        }
        catch (RestException e)
        {
            Console.WriteLine($"Error: {e.Code} - {e.ErrorMessage}");
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error: {e.Message}");
            return null;
        }
    }

    public async Task<List<Competitor>?> GetTopCompetitors(int contest)
    {
        try
        {
            var response = await codeforcesClient.GetTopNCompetitors(contest);

            if (response == null)
            {
                throw new RestException(HttpStatusCode.NotFound, "Failed to retrieve the top competitors");
            }

            return response;
        }
        catch (RestException e)
        {
            Console.WriteLine($"Error: {e.Code} - {e.ErrorMessage}");
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error: {e.Message}");
            return null;
        }
    }
}