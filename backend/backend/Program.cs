using System.Net;
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
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();

        string? mongoConnectionString = builder.Configuration["MONGO_CONNECTION_STRING"];
        string? mongoDatabaseName = builder.Configuration["MONGO_DATABASE_NAME"];
        string? jwtSecret = builder.Configuration["JWT_SECRET"];

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

            builder.Services.AddCors(o => o.AddPolicy("policy", policyBuilder =>
            {
                policyBuilder.WithOrigins("*")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));


            // utils
            builder.Services.AddSingleton(new MongoDbContext(mongoConnectionString!, mongoDatabaseName!));
            builder.Services.AddSingleton(new JwtService(jwtSecret!));
            builder.Services.AddHttpClient();

            // clients
            builder.Services.AddSingleton<ICodeforcesClient, CodeforcesClient>();

            // services
            builder.Services.AddSingleton<MongoDbService>();

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
                    return Results.Ok(new { Token = token, Username = request.Username! });
                }
                catch (RestException ex)
                {
                    return Results.Problem(detail: ex.ErrorMessage, statusCode: (int)ex.Code);
                }
            });

        app.MapPost("/login", async (LoginService loginService, UserLoginRequest request) =>
        {
            try
            {
                string token = await loginService.Login(request.Username!, request.Password!);
                return Results.Ok(new { Token = token, Username = request.Username! });
            }
            catch (RestException ex)
            {
                return Results.Problem(detail: ex.ErrorMessage, statusCode: (int)ex.Code);
            }
        });


        app.MapGet("/user/{username}", async (MongoDbService service, string username) =>
        {
            try
            {
                var user = await service.GetUser(username);
                return Results.Ok(user);
            }
            catch (RestException ex)
            {
                return Results.Problem(detail: ex.ErrorMessage, statusCode: (int)ex.Code);
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
                return Results.Problem(detail: ex.ErrorMessage, statusCode: (int)ex.Code);
            }
        });

        app.MapGet("/conteststatus/{id:int}", async (MongoDbService service, int id) =>
        {
            try
            {
                var status = await service.GetContestStatus(id);
                return Results.Ok(status);
            }
            catch (RestException ex)
            {
                return Results.Problem(detail: ex.ErrorMessage, statusCode: (int)ex.Code);
            }
        });

        app.MapGet("/userbets/{username}",
            async (MongoDbService service, string username) =>
            {
                try
                {
                    var result = await service.GetUserBets(username);

                    return Results.Ok(result);
                }
                catch (RestException ex)
                {
                    return Results.Problem(detail: ex.ErrorMessage, statusCode: (int)ex.Code);
                }
            });


        app.MapGet("/usercontestbets/{username}:{contestId:int}",
            async (MongoDbService service, string username, int contestId) =>
            {
                try
                {
                    var result = await service.GetUserContestBets(username, contestId);

                    return Results.Ok(result);
                }
                catch (RestException ex)
                {
                    return Results.Problem(detail: ex.ErrorMessage, statusCode: (int)ex.Code);
                }
            });

        app.MapGet("/contests", async (MongoDbService service) =>
        {
            try
            {
                var result = await service.GetCurrentContests();

                return Results.Ok(result);
            }
            catch (RestException ex)
            {
                return Results.Problem(detail: ex.ErrorMessage, statusCode: (int)ex.Code);
            }
        });

        app.MapGet("/contestbets/{id:int}", async (MongoDbService service, int id) =>
        {
            try
            {
                var result = await service.GetContestBets(id);

                return Results.Ok(result);
            }
            catch (RestException ex)
            {
                return Results.Problem(detail: ex.ErrorMessage, statusCode: (int)ex.Code);
            }
        });

        app.MapGet("/rankings", async (MongoDbService service) =>
        {
            try
            {
                var result = await service.GetRankings();

                return Results.Ok(result);
            }
            catch (RestException ex)
            {
                return Results.Problem(detail: ex.ErrorMessage, statusCode: (int)ex.Code);
            }
        });

        app.MapPost("/bet/winner/details",
            async (HttpRequest request, JwtService jwtService, BetWinnerService winnerService,
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
                    betRequest.BetType = BetType.Winner;

                    var result = await winnerService.GetBetDetails(betRequest);
                    return Results.Accepted(value: OddsConverter
                        .GetAmericanOddsFromProbability((double)result.Probability!)
                        .ToString());
                }
                catch (RestException ex)
                {
                    return Results.Problem(detail: ex.ErrorMessage, statusCode: (int)HttpStatusCode.NotFound);
                }
            }).RequireAuthorization();

        app.MapPost("/bet/topn/details",
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
                    betRequest.BetType = BetType.TopN;

                    var result = await topNService.GetBetDetails(betRequest);
                    return Results.Accepted(value: OddsConverter
                        .GetAmericanOddsFromProbability((double)result.Probability!)
                        .ToString());
                }
                catch (RestException ex)
                {
                    return Results.Problem(detail: ex.ErrorMessage, statusCode: (int)ex.Code);
                }
            }).RequireAuthorization();

        app.MapPost("/bet/compete/details",
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

                    var result = await competeService.GetBetDetails(betRequest);
                    return Results.Accepted(value: OddsConverter
                        .GetAmericanOddsFromProbability((double)result.Probability!)
                        .ToString());
                }
                catch (RestException ex)
                {
                    return Results.Problem(detail: ex.ErrorMessage, statusCode: (int)ex.Code);
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
                    betRequest.BetType = BetType.Winner;
                    betRequest.Date = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    await winnerService.PlaceBet(betRequest);
                    return Results.Accepted();
                }
                catch (RestException ex)
                {
                    return Results.Problem(detail: ex.ErrorMessage, statusCode: (int)HttpStatusCode.NotFound);
                }
            }).RequireAuthorization();

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
                    betRequest.Date = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    await competeService.PlaceBet(betRequest);
                    return Results.Accepted();
                }
                catch (RestException ex)
                {
                    return Results.Problem(detail: ex.ErrorMessage, statusCode: (int)ex.Code);
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
                    betRequest.BetType = BetType.TopN;
                    betRequest.Date = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    await topNService.PlaceBet(betRequest);
                    return Results.Accepted();
                }
                catch (RestException ex)
                {
                    return Results.Problem(detail: ex.ErrorMessage, statusCode: (int)ex.Code);
                }
            }).RequireAuthorization();


        app.UseCors("policy");

        var cur1 = app.RunAsync();

        var cur2 = Task.Run(async () =>
        {
            var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));

            using var scope = app.Services.CreateScope();
            var updateService = scope.ServiceProvider.GetRequiredService<UpdateService>();

            do
            {
                Console.WriteLine("Checking contests...");
                await updateService.CheckContests();
                Console.WriteLine("Updating contests...");
                await updateService.UpdateContests();
                Console.WriteLine("Getting competitors...");
                await updateService.GetCompetitors();
                Console.WriteLine("Done");
            } while (await timer.WaitForNextTickAsync());
        });

        await Task.WhenAll(cur1, cur2);
    }
}