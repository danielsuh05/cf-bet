using System.Net;
using backend.clients;
using backend.interfaces;
using backend.results.codeforces;
using backend.results.db;
using backend.services;
using backend.utils;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

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
        var client = new HttpClient();

        builder.Services.AddSingleton(new MongoDBContext(mongoConnectionString, mongoDatabaseName));
        builder.Services.AddSingleton(new JwtService(jwtSecret));
        builder.Services.AddSingleton(new CodeforcesClient(client));

        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<ICodeforcesClient, CodeforcesClient>();

        builder.Services.AddScoped<ContestService>();
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
                string token = await loginService.Register(request.Username!, request.Password!);
                return Results.Ok(new { Token = token });
            }
            catch (RestException ex)
            {
                return Results.Problem(ex.Message, statusCode: (int)ex.Code!);
            }
        });

        app.MapPost("/login", async (LoginService loginService, UserLoginRequest request) =>
        {
            try
            {
                string token = await loginService.Login(request.Username!, request.Password!);
                return Results.Ok(new { Token = token });
            }
            catch (RestException ex)
            {
                return Results.Problem(ex.Message, statusCode: (int)ex.Code!);
            }
        });

        app.MapGet("/contests/{id:int}", async (int id, ContestService contestService) =>
        {
            try
            {
                var competitors = await contestService.GetTopCompetitors(id);
                return Results.Ok(new { Competitors = competitors });
            }
            catch (RestException ex)
            {
                return Results.Problem(ex.Message, statusCode: (int)ex.Code!);
            }
        });

        app.UseHttpsRedirection();
        app.Run();
    }
}