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
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonConvert.DeserializeObject<UserResultResponse>(responseBody);
            return apiResponse?.Result?[0];
        }
        catch (HttpRequestException e)
        {
            throw new RestException(e.StatusCode, e.Message);
        }
    }

    public async Task<List<Contest?>> GetCurrentContests()
    {
        try
        {
            const string apiUrl = $"https://codeforces.com/api/contest.list?gym=false";
            var response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonConvert.DeserializeObject<ContestListInfo>(responseBody);
            return apiResponse?.Result!;
        }
        catch (HttpRequestException e)
        {
            throw new RestException(e.StatusCode, e.Message);
        }
    }

    /// <summary>
    /// Get the top n competitors using web scraping from the contestRegistrants Codeforces HTML Document.
    /// </summary>
    /// <param name="html">string containing the HTML Document for contestRegistrants</param>
    /// <param name="n">how many competitors to return</param>
    /// <returns>list of the top n competitors</returns>
    private List<Competitor> ParseContestRegistrantHtml(string html, int n)
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

    /// <summary>
    /// Gets the top n competitors from a certain contest.
    /// </summary>
    /// <param name="id">id</param>
    /// <returns>List of the top n competitors</returns>
    public async Task<List<Competitor>?> GetTopNCompetitors(int id)
    {
        try
        {
            var fullUrl = $"https://codeforces.com/contestRegistrants/{id}?order=BY_RATING_DESC";
            var response = await client.GetAsync(fullUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            // max value is 250
            const int n = 250;

            return ParseContestRegistrantHtml(responseBody, n);
        }
        catch (HttpRequestException e)
        {
            throw new RestException(e.StatusCode, e.Message);
        }
    }
}