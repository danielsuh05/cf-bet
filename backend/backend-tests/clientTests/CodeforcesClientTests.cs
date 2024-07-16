using System.Net;
using backend.clients;
using backend.interfaces;
using backend.results.codeforces;
using backend.utils;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace backend_tests.clientTests
{
    [TestFixture]
    public class CodeforcesClientTests
    {
        private Mock<HttpMessageHandler> _handlerMock;
        private HttpClient _httpClient;
        private ICodeforcesClient _codeforcesClient;

        [SetUp]
        public void Setup()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_handlerMock.Object);
            _codeforcesClient = new CodeforcesClient(_httpClient);
        }

        [Test]
        public async Task GetUserInfo_ReturnsUserResultResponse_OnSuccess()
        {
            const string username = "danielsuh";
            var userResultResponse = new UserResultResponse
            {
                Status = "OK",
                Result = new UserResult[]
                    { new UserResult { FirstName = "daniel", LastName = "suh", Handle = username } }
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

            var result = await _codeforcesClient.GetUserInfo(username);

            result.Should().NotBeNull();
            result.Handle!.ToLower().Should().Be(username);
        }

        [Test]
        public async Task GetUserInfo_ThrowsRestException_OnHttpRequestException()
        {
            const string username = "danielsuhtest";

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            Func<Task> act = async () => await _codeforcesClient.GetUserInfo(username);

            await act.Should().ThrowAsync<RestException>();
        }
    }
}