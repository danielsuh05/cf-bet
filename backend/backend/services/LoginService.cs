using System.Net;
using backend.interfaces;
using backend.results.db;
using backend.utils;
using MongoDB.Driver;

namespace backend.services;

public class LoginService(MongoDBContext context, JwtService jwtService, ICodeforcesClient codeforcesClient)
{
    private async Task<bool> UserExists(string username)
    {
        var user = await context.Users.Find(u => u.Username == username).FirstOrDefaultAsync();
        return user != null;
    }

    public async Task<string> Register(string username, string password)
    {
        var response = await codeforcesClient.GetUserInfo(username);
        if (response == null)
        {
            throw new RestException(HttpStatusCode.Unauthorized, "Error getting data from Codeforces.");
        }

        if (response == null)
        {
            throw new RestException(HttpStatusCode.NotFound, $"Error finding username {username}.");
        }

        // TODO: add back in
        // if (response.Result[0].FirstName != "cfbet")
        // {
        //     throw new RestException(HttpStatusCode.Unauthorized, "Please set your first name to \"cfbet\" in Codeforces.");
        // }

        if (await UserExists(username))
        {
            throw new RestException(HttpStatusCode.Conflict, $"{username} is already registered for cf-bet.");
        }

        string? passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new UserSchema
        {
            Username = username,
            PasswordHash = passwordHash
        };

        await context.Users.InsertOneAsync(user);

        return jwtService.GenerateToken(user.Id!);
    }

    public async Task<string> Login(string username, string password)
    {
        var user = await context.Users.Find(u => u.Username == username).FirstOrDefaultAsync();
        if (user == null)
        {
            throw new RestException(HttpStatusCode.Unauthorized, "Invalid username or password.");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            throw new RestException(HttpStatusCode.Unauthorized, "Invalid username or password.");
        }

        return jwtService.GenerateToken(user.Id!);
    }
}