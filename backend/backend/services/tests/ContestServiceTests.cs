using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using backend.api;
using backend.results.codeforces;
using Moq;
using Xunit;

namespace backend.services.Tests
{
    public class ContestServiceTests
    {
        private readonly Mock<CodeforcesApi> _mockCodeforcesApi;
        private readonly ContestService _contestService;

        public ContestServiceTests()
        {
            _mockCodeforcesApi = new Mock<ICodeforcesApi>();
            _contestService = new ContestService(_mockCodeforcesApi.Object);
        }

        [Fact]
        public async Task GetActiveContests_ReturnsActiveContests_WhenApiReturnsValidData()
        {
            // Arrange
            var contests = new List<Contest>
            {
                new Contest { Id = 1, Name = "Contest 1", Phase = "BEFORE" },
                new Contest { Id = 2, Name = "Contest 2", Phase = "FINISHED" },
                new Contest { Id = 3, Name = "Contest 3", Phase = "BEFORE" }
            };

            var apiResponse = new ApiResponse<List<Contest>>
            {
                Status = "OK",
                Result = contests
            };

            _mockCodeforcesApi.Setup(api => api.GetContestInfo())
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _contestService.GetActiveContests();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, contest => Assert.Equal("BEFORE", contest.Phase));
        }

        [Fact]
        public async Task GetActiveContests_ReturnsNull_WhenApiResponseIsNull()
        {
            // Arrange
            _mockCodeforcesApi.Setup(api => api.GetContestInfo())
                .ReturnsAsync((ApiResponse<List<Contest>>?)null);

            // Act
            var result = await _contestService.GetActiveContests();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetActiveContests_ReturnsNull_WhenApiResponseStatusIsNotOk()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<Contest>>
            {
                Status = "FAILED",
                Result = null
            };

            _mockCodeforcesApi.Setup(api => api.GetContestInfo())
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _contestService.GetActiveContests();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetActiveContests_ReturnsNull_AndLogsError_WhenRestExceptionIsThrown()
        {
            // Arrange
            _mockCodeforcesApi.Setup(api => api.GetContestInfo())
                .ThrowsAsync(new RestException(HttpStatusCode.NotFound, "Failed to retrieve contest information"));

            // Act
            var result = await _contestService.GetActiveContests();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetActiveContests_ReturnsNull_AndLogsError_WhenExceptionIsThrown()
        {
            // Arrange
            _mockCodeforcesApi.Setup(api => api.GetContestInfo())
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _contestService.GetActiveContests();

            // Assert
            Assert.Null(result);
        }
    }
}
