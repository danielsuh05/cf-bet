using backend.api;
using backend.results;

namespace backend.services;

public class Login
{
    public async Task register(string username, string password)
    {
        var response = await CodeforcesApi.GetUserInfo(username);
        if (response == null)
        {
            return 
        }
    }
}