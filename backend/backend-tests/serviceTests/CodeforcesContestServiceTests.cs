using backend.interfaces;
using backend.results.codeforces;
using backend.services;
using FluentAssertions;
using Moq;

namespace backend_tests.serviceTests
{
    [TestFixture]
    public class CodeforcesContestServiceTests
    {
        private Mock<ICodeforcesClient> _codeforcesClientMock;
        private CodeforcesContestService _codeforcesContestService;

        [SetUp]
        public void Setup()
        {
            _codeforcesClientMock = new Mock<ICodeforcesClient>();
            _codeforcesContestService = new CodeforcesContestService(_codeforcesClientMock.Object);
        }

        [Test]
        public async Task GetActiveContests_ReturnsActiveContests_OnSuccess()
        {
            var contestList = new List<Contest>
            {
                new() { Id = 1, Name = "Contest 1", Phase = "BEFORE" },
                new() { Id = 2, Name = "Contest 2", Phase = "FINISHED" },
                new() { Id = 3, Name = "Contest 3", Phase = "BEFORE" }
            };

            _codeforcesClientMock.Setup(client => client.GetCurrentContests())!
                .ReturnsAsync(contestList);

            var result = await _codeforcesContestService.GetActiveContests();

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.Name == "Contest 1");
            result.Should().Contain(c => c.Name == "Contest 3");
        }

        [Test]
        public async Task GetActiveContests_ThrowsRestException_OnFailedResponse()
        {
            _codeforcesClientMock.Setup(client => client.GetCurrentContests())!
                .ReturnsAsync((List<Contest>?)null);

            var result = await _codeforcesContestService.GetActiveContests();

            result.Should().BeNull();
        }

        [Test]
        public async Task GetActiveContests_ThrowsRestException_OnErrorResponse()
        {
            List<Contest?>? contestList = null;

            _codeforcesClientMock.Setup(client => client.GetCurrentContests())!
                .ReturnsAsync(contestList);

            var result = await _codeforcesContestService.GetActiveContests();

            result.Should().BeNull();
        }

        [Test]
        public async Task GetActiveContests_ReturnsNull_OnUnexpectedException()
        {
            _codeforcesClientMock.Setup(client => client.GetCurrentContests())
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _codeforcesContestService.GetActiveContests();

            result.Should().BeNull();
        }
    }
}