using backend.results.db;
using backend.services;
using backend.utils;
using Microsoft.AspNetCore.Identity.Data;

namespace backend;

public static class Program
{
    public static void Main(string[] args)
    {
        string root = Directory.GetCurrentDirectory();
        string dotenv = Path.Combine(root, ".env");
        DotEnv.Load(dotenv);

        var builder = WebApplication.CreateBuilder(args);

        string? mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
        string? mongoDatabaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME");
        string? jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");

        builder.Services.AddSingleton(new MongoDBContext(mongoConnectionString, mongoDatabaseName));
        builder.Services.AddSingleton(new JwtService(jwtSecret));
        builder.Services.AddScoped<LoginService>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapPost("/register", async (LoginService loginService, UserLoginRequest request) =>
        {
            try
            {
                string token = await loginService.Register(request.Username, request.Password);
                return Results.Ok(new { Token = token });
            }
            catch (RestException ex)
            {
                return Results.Problem(ex.Message, statusCode: (int)ex.Code);
            }
        });
        
        app.MapPost("/login", async (LoginService loginService, UserLoginRequest request) =>
        {
            try
            {
                var token = await loginService.Login(request.Username, request.Password);
                return Results.Ok(new { Token = token });
            }
            catch (RestException ex)
            {
                return Results.Problem(ex.Message, statusCode: (int)ex.Code);
            }
        });

        app.UseHttpsRedirection();
        app.Run();
    }
}