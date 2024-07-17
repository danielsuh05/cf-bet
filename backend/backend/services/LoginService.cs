using System.Net;
using backend.interfaces;
using backend.results.db;
using backend.utils;
using MongoDB.Driver;

namespace backend.services;

public class LoginService(ICodeforcesClient codeforcesClient, MongoDbContext context, JwtService jwtService)
{
    private async Task<bool> UserExists(string username)
    {
        var user = await context.Users.Find(u => u.Username == username).FirstOrDefaultAsync();
        return user != null;
    }

    public async Task<string> Register(string username, string password)
    {
        try
        {
            var response = await codeforcesClient.GetUserInfo(username);
            if (response == null)
            {
                throw new RestException(HttpStatusCode.Unauthorized, $"Error finding username {username}.");
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
                PasswordHash = passwordHash,
                MoneyBalance = 1_000
            };

            await context.Users.InsertOneAsync(user);

            return jwtService.GenerateToken(user);
        }
        catch (RestException e)
        {
            throw new RestException(e.Code, e.ErrorMessage);
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<string> Login(string username, string password)
    {
        try
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

            return jwtService.GenerateToken(user);
        }
        catch (RestException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }
}