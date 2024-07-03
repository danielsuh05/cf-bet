using System.Net;
using backend.api;
using backend.results;
using backend.results.db;
using backend.utils;
using MongoDB.Driver;

namespace backend.services;

public class LoginService
{
    private readonly MongoDBContext _context;
    private readonly JwtService _jwtService;

    public LoginService(MongoDBContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }
    
    public async Task<string> Register(string username, string password)
    {
        var response = await CodeforcesApi.GetUserInfo(username);
        if (response == null)
        {
            throw new RestException(HttpStatusCode.Unauthorized, "Error getting data from Codeforces.");
        }

        if (response.Status != "OK" || response.Result.Length > 1)
        {
            throw new RestException(HttpStatusCode.NotFound, $"Error finding username {username}");
        }

        // TODO: add back in
        // if (response.Result[0].FirstName != "cfbet")
        // {
        //     throw new RestException(HttpStatusCode.Unauthorized, "Please set your first name to \"cfbet\" in Codeforces.");
        // }

        string? passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Username = username,
            PasswordHash = passwordHash
        };

        await _context.Users.InsertOneAsync(user);

        return _jwtService.GenerateToken(user.Id);
    }
    
    public async Task<string> Login(string username, string password)
    {
        var user = await _context.Users.Find(u => u.Username == username).FirstOrDefaultAsync();
        if (user == null)
        {
            throw new RestException(HttpStatusCode.Unauthorized, "Invalid username or password.");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            throw new RestException(HttpStatusCode.Unauthorized, "Invalid username or password.");
        }

        return _jwtService.GenerateToken(user.Id);
    }
}