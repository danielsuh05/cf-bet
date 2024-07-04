using backend.interfaces;
using backend.results.codeforces;
using backend.services;
using FluentAssertions;
using Moq;

namespace backend_tests.serviceTests
{
    [TestFixture]
    public class ContestServiceTests
    {
        private Mock<ICodeforcesClient> _codeforcesClientMock;
        private ContestService _contestService;

        [SetUp]
        public void Setup()
        {
            _codeforcesClientMock = new Mock<ICodeforcesClient>();
            _contestService = new ContestService(_codeforcesClientMock.Object);
        }

        [Test]
        public async Task GetActiveContests_ReturnsActiveContests_OnSuccess()
        {
            var contestListResponse = new ContestListInfo
            {
                Status = "OK",
                Result = new List<Contest>
                {
                    new() { Id = 1, Name = "Contest 1", Phase = "BEFORE" },
                    new() { Id = 2, Name = "Contest 2", Phase = "FINISHED" },
                    new() { Id = 3, Name = "Contest 3", Phase = "BEFORE" }
                }
            };

            _codeforcesClientMock.Setup(client => client.GetCurrentContests())
                .ReturnsAsync(contestListResponse);

            var result = await _contestService.GetActiveContests();

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.Name == "Contest 1");
            result.Should().Contain(c => c.Name == "Contest 3");
        }

        [Test]
        public async Task GetActiveContests_ThrowsRestException_OnFailedResponse()
        {
            _codeforcesClientMock.Setup(client => client.GetCurrentContests())
                .ReturnsAsync((ContestListInfo?)null);

            var result = await _contestService.GetActiveContests();

            result.Should().BeNull();
        }

        [Test]
        public async Task GetActiveContests_ThrowsRestException_OnErrorResponse()
        {
            var contestListResponse = new ContestListInfo
            {
                Status = "FAILED",
                Result = null
            };

            _codeforcesClientMock.Setup(client => client.GetCurrentContests())
                .ReturnsAsync(contestListResponse);

            var result = await _contestService.GetActiveContests();

            result.Should().BeNull();
        }

        [Test]
        public async Task GetActiveContests_ReturnsNull_OnUnexpectedException()
        {
            _codeforcesClientMock.Setup(client => client.GetCurrentContests())
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _contestService.GetActiveContests();

            result.Should().BeNull();
        }
    }
}