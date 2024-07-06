using System.Text;
using backend.clients;
using backend.interfaces;
using backend.results.db;
using backend.results.requests;
using backend.services;
using backend.utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

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

        builder.Services.AddAuthentication(cfg =>
        {
            cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            cfg.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = false;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8
                        .GetBytes(jwtSecret!)
                ),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddAuthorization();

        builder.Services.AddSingleton(new MongoDBContext(mongoConnectionString, mongoDatabaseName));
        builder.Services.AddSingleton(new JwtService(jwtSecret));

        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<CodeforcesClient>();

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

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapPost(
            "/register", async (LoginService loginService, UserLoginRequest request) =>
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

        app.MapGet("/contests/{id:int}", async (ContestService contestService, int id) =>
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

        app.MapPost("/bet", async (BetRequest request) =>
        {
            var service = new BetService(request);
            var result = await service.PlaceBet();
            return result;
        }).RequireAuthorization();

        app.UseHttpsRedirection();
        app.Run();
    }
}