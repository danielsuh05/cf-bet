using System.Text;
using backend.clients;
using backend.interfaces;
using backend.results.db;
using backend.results.requests;
using backend.services;
using backend.services.betServices;
using backend.utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        {
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

            // utils
            builder.Services.AddSingleton(new MongoDbContext(mongoConnectionString!, mongoDatabaseName!));
            builder.Services.AddSingleton(new JwtService(jwtSecret!));
            builder.Services.AddHttpClient();

            // clients
            builder.Services.AddSingleton<ICodeforcesClient, CodeforcesClient>();

            // services
            builder.Services.AddSingleton<MongoDbService>();
            builder.Services.AddSingleton<CodeforcesContestService>();

            // bet services
            builder.Services.AddSingleton<BetCompeteService>();
            builder.Services.AddSingleton<BetWinnerService>();
            builder.Services.AddSingleton<BetTopNService>();

            builder.Services.AddSingleton<LoginService>();
            builder.Services.AddSingleton<UpdateService>();

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
        }

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

        app.MapGet("/contestcompetitors/{id:int}", async (MongoDbService service, int id) =>
        {
            try
            {
                var competitors = await service.GetTopNCompetitorsFromDb(id);
                return Results.Ok(new { Competitors = competitors });
            }
            catch (RestException ex)
            {
                return Results.Problem(ex.Message, statusCode: (int)ex.Code!);
            }
        });

        app.MapGet("/userbets/{userid}",
            async (MongoDbService service, string userid) =>
            {
                try
                {
                    var result = await service.GetUserBets(userid);

                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });


        app.MapGet("/usercontestbets/{userid}:{contestId:int}",
            async (MongoDbService service, string userid, int contestId) =>
            {
                try
                {
                    var result = await service.GetUserContestBets(userid, contestId);

                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });

        app.MapGet("/contests", async (MongoDbService service) =>
        {
            try
            {
                var result = await service.GetCurrentContests();

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        app.MapGet("/contestbets/{id:int}", async (MongoDbService service, int id) =>
        {
            try
            {
                var result = await service.GetContestBets(id);

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        app.MapGet("/rankings", async (MongoDbService service) =>
        {
            try
            {
                var result = await service.GetRankings();

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        app.MapPost("/bet/compete",
            async (HttpRequest request, JwtService jwtService, BetCompeteService competeService,
                BetSchema betRequest) =>
            {
                try
                {
                    string token = request.Headers.Authorization.ToString().Replace("Bearer ", "");

                    string userId = jwtService.GetUserId(token);
                    string userName = jwtService.GetUserName(token);
                    betRequest.Id = null;
                    betRequest.UserId = userId;
                    betRequest.Username = userName;
                    betRequest.BetType = BetType.Compete;

                    await competeService.PlaceBet(betRequest);
                    return Results.Accepted();
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            }).RequireAuthorization();

        app.MapPost("/bet/winner",
            async (HttpRequest request, JwtService jwtService, BetWinnerService winnerService, BetSchema betRequest) =>
            {
                try
                {
                    string token = request.Headers.Authorization.ToString().Replace("Bearer ", "");

                    string userId = jwtService.GetUserId(token);
                    string userName = jwtService.GetUserName(token);
                    betRequest.Id = null;
                    betRequest.UserId = userId;
                    betRequest.Username = userName;
                    betRequest.BetType = BetType.Compete;

                    await winnerService.PlaceBet(betRequest);
                    return Results.Accepted();
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            }).RequireAuthorization();

        app.MapPost("/bet/topn",
            async (HttpRequest request, JwtService jwtService, BetTopNService topNService, BetSchema betRequest) =>
            {
                try
                {
                    string token = request.Headers.Authorization.ToString().Replace("Bearer ", "");

                    string userId = jwtService.GetUserId(token);
                    string userName = jwtService.GetUserName(token);
                    betRequest.Id = null;
                    betRequest.UserId = userId;
                    betRequest.Username = userName;
                    betRequest.BetType = BetType.Compete;

                    await topNService.PlaceBet(betRequest);
                    return Results.Accepted();
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            }).RequireAuthorization();

        var cur1 = app.RunAsync();
        var cur2 = Task.Run(async () =>
        {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

            using var scope = app.Services.CreateScope();
            var updateService = scope.ServiceProvider.GetRequiredService<UpdateService>();

            while (await timer.WaitForNextTickAsync())
            {
                Console.WriteLine("Checking contests...");
                await updateService.CheckContests();
                Console.WriteLine("Updating contests...");
                await updateService.UpdateContests();
                Console.WriteLine("Getting competitors...");
                await updateService.GetCompetitors();
                Console.WriteLine("Done");

                break;
            }
        });

        await Task.WhenAll(cur1, cur2);
    }
}