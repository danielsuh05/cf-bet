using System.Net;
using backend.results.codeforces;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace backend.api.tests;

public class CodeforcesApiTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;

    public CodeforcesApiTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object);
    }

    [Fact]
    public async Task GetUserInfo_ReturnsUserResultResponse_OnSuccess()
    {
        const string username = "danielsuh";
        var userResultResponse = new UserResultResponse
        {
            Status = "OK",
            Result =
            [
                new UserResult { FirstName = "daniel", LastName = "suh", Handle = username }
            ]
        };

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(userResultResponse))
            });

        var result = await CodeforcesApi.GetUserInfo(username);

        result.Should().NotBeNull();
        result.Status.Should().Be("OK");
        result.Result.Should().HaveCount(1);
        result.Result[0].Handle!.ToLower().Should().Be(username);
    }

    [Fact]
    public async Task GetUserInfo_ReturnsNull_OnHttpRequestException()
    {
        const string username = "danielsuhtest";

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        var result = await CodeforcesApi.GetUserInfo(username);

        result.Should().BeNull();
    }

    // TODO: Add contest tests
}