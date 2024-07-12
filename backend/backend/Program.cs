using System.Text;
using backend.clients;
using backend.interfaces;
using backend.results.betting;
using backend.results.codeforces;
using backend.results.db;
using backend.services;
using backend.services.betServices;
using backend.utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace backend;

public static class Program
{
    public static async Task Main(string[] args)
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
        builder.Services.AddSingleton<ICodeforcesClient, CodeforcesClient>();

        builder.Services.AddScoped<ContestService>();
        builder.Services.AddScoped<CompeteService>();
        builder.Services.AddScoped<LoginService>();
        builder.Services.AddScoped<UpdateService>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(opt =>
        {
            opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
            opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });

            opt.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        using (var scope = app.Services.CreateScope())
        {
            var updateService = scope.ServiceProvider.GetRequiredService<UpdateService>();

            while (await timer.WaitForNextTickAsync())
            {
                /*
                check for new contests
                update the leaderboard
                check if any contest is completed but P/L hasn't been applied
                yes:
                    1) get results of that contest
                    2) update all bet objects and users in that contest
                    3) close contest
                no:
                    do nothing
                if contest is in betting stage:
                    1) get competitors and store in database (update if already in database)
                */
                Console.WriteLine("Updating Contest Information...");
                await updateService.CheckContests();
            }
        }

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

        app.MapPost("/bet/compete",
            async (CompeteService competeService, JwtService jwtService, HttpRequest request, BetEntry betRequest) =>
            {
                string token = request.Headers.Authorization.ToString().Replace("Bearer ", "");

                betRequest.UserId = jwtService.GetUserId(token);

                await competeService.PlaceBet(betRequest);
            }).RequireAuthorization();

        app.UseHttpsRedirection();
        app.Run();
    }
}