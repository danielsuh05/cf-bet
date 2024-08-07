using System.Net;
using backend.interfaces;
using backend.results.codeforces;
using backend.utils;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace backend.clients;

public class CodeforcesClient(HttpClient client) : ICodeforcesClient
{
    public async Task<UserResult?> GetUserInfo(string username)
    {
        try
        {
            var apiUrl = $"https://codeforces.com/api/user.info?handles={username}";
            var response = await client.GetAsync(apiUrl);
            string responseBody = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonConvert.DeserializeObject<UserResultResponse>(responseBody);

            if (apiResponse?.Result == null || apiResponse.Result.Length == 0 || apiResponse.Status != "OK")
            {
                throw new RestException(HttpStatusCode.NotFound, $"Could not find user with handle ${username}");
            }

            return apiResponse.Result.First();
        }
        catch (RestException)
        {
            throw;
        }
        catch (HttpRequestException e) when (e.StatusCode.HasValue)
        {
            throw new RestException(e.StatusCode.Value, e.Message);
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<List<Contest>> GetCurrentContests()
    {
        try
        {
            const string apiUrl = $"https://codeforces.com/api/contest.list?gym=false";
            var response = await client.GetAsync(apiUrl);
            string responseBody = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonConvert.DeserializeObject<ContestListInfo>(responseBody);
            if (apiResponse?.Result == null || apiResponse.Status != "OK")
            {
                throw new RestException(HttpStatusCode.NotFound, "Could not get the current Codeforces contests.");
            }

            return apiResponse.Result;
        }
        catch (RestException)
        {
            throw;
        }
        catch (HttpRequestException e) when (e.StatusCode.HasValue)
        {
            throw new RestException(e.StatusCode.Value, e.Message);
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    private List<Competitor> ParseContestRegistrantHtml(string html, int n)
    {
        try
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var topCompetitors = new List<Competitor>();

            var table = document.DocumentNode.SelectSingleNode("//table[@class='registrants']");

            if (table == null) return [];
            var rows = table.SelectNodes("tr");
            if (rows == null) return [];

            var numContestants = 0;
            foreach (var row in rows.Skip(1)) // Skip the header row
            {
                if (numContestants == n) break;
                numContestants++;

                var cells = row.SelectNodes("td");
                if (cells == null || cells.Count < 3) continue;
                var usernameNode = cells[1].SelectSingleNode("a");
                var ratingNode = cells[2];

                if (usernameNode == null || ratingNode == null) continue;
                string username = usernameNode.InnerText.Trim();
                int rating = int.Parse(ratingNode.InnerText.Trim());

                var curCompetitor = new Competitor
                {
                    Handle = username,
                    Ranking = rating
                };
                topCompetitors.Add(curCompetitor);
            }

            return topCompetitors;
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<List<Competitor>?> GetTopNCompetitors(int id)
    {
        try
        {
            var fullUrl = $"https://codeforces.com/contestRegistrants/{id}?order=BY_RATING_DESC";
            var response = await client.GetAsync(fullUrl);

            string responseBody = await response.Content.ReadAsStringAsync();

            // number of competitors to return
            const int n = 250;

            return ParseContestRegistrantHtml(responseBody, n);
        }
        catch (HttpRequestException e) when (e.StatusCode.HasValue)
        {
            throw new RestException(e.StatusCode.Value, e.Message);
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<List<Member>?> GetRankings(int id)
    {
        try
        {
            var url =
                $"https://codeforces.com/api/contest.standings?contestId={id}&asManager=false&from=1&count=250&showUnofficial=true";
            var response = await client.GetAsync(url);

            string responseBody = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonConvert.DeserializeObject<ContestInfo>(responseBody);
            if (apiResponse == null || apiResponse.Status != "OK")
            {
                throw new RestException(HttpStatusCode.NotFound, "Did not find the contest from Codeforces API");
            }

            var results = apiResponse.Result!.Rows!.Select(row => row.Party!.Members!.First()).ToList();
            return results;
        }
        catch (RestException)
        {
            throw;
        }
        catch (HttpRequestException e) when (e.StatusCode.HasValue)
        {
            throw new RestException(e.StatusCode.Value, e.Message);
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<int> GetRankingHandle(string handle)
    {
        try
        {
            var url =
                $"https://codeforces.com/api/contest.standings?contestId=1988&asManager=false&from=1&count=5&showUnofficial=true&handles={handle}";

            var response = await client.GetAsync(url);

            string responseBody = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonConvert.DeserializeObject<ContestInfo>(responseBody);
            if (apiResponse == null || apiResponse.Status != "OK")
            {
                throw new RestException(HttpStatusCode.NotFound, "Did not find the contest from Codeforces API");
            }

            if (apiResponse.Result!.Rows!.Length == 0) return -1;

            int result = apiResponse.Result!.Rows!.First().Rank;
            return result;
        }
        catch (RestException)
        {
            throw;
        }
        catch (HttpRequestException e) when (e.StatusCode.HasValue)
        {
            throw new RestException(e.StatusCode.Value, e.Message);
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }
}