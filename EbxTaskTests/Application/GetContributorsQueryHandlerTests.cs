using Application.Contributors.Queries;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;

namespace EbxTask.Tests.Application.Handlers;

public class GetContributorsQueryHandlerTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<GetContributorsQueryHandler>> _loggerMock;
    private readonly GetContributorsQueryHandler _handler;

    public GetContributorsQueryHandlerTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _loggerMock = new Mock<ILogger<GetContributorsQueryHandler>>();
        _handler = new GetContributorsQueryHandler(_httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRepositoryIsNotFound()
    {
        // Arrange
        var query = new GetContributorsQuery { Owner = "testOwner", Repo = "testRepo" };
        var url = $"https://api.github.com/repos/{query.Owner}/{query.Repo}/commits?per_page=100";
        
        _httpMessageHandlerMock.SetupRequest(HttpMethod.Get, url)
            .ReturnsResponse(HttpStatusCode.NotFound);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Repository 'testOwner/testRepo' not found", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenApiCallFails()
    {
        // Arrange
        var query = new GetContributorsQuery { Owner = "testOwner", Repo = "testRepo" };
        var url = $"https://api.github.com/repos/{query.Owner}/{query.Repo}/commits?per_page=100";

        // Mock HTTP response (500 Internal Server Error)
        _httpMessageHandlerMock.SetupRequest(HttpMethod.Get, url)
            .ReturnsResponse(HttpStatusCode.InternalServerError, "Internal Server Error");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Error fetching contributors: Internal Server Error", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenContributorsAreFound()
    {
        // Arrange
        var query = new GetContributorsQuery { Owner = "testOwner", Repo = "testRepo" };
        var url = $"https://api.github.com/repos/{query.Owner}/{query.Repo}/commits?per_page=100";

        var commits = new List<object>
        {
            new { sha = "7fd1a60b01f91b314f59955a4e4d4e80d8edf11d", author = new { login = "user1" }, committer = new { login = "committer1" } },
            new { sha = "762941318ee16e59dabbacb1b4049eec22f0d303", author = new { login = "user2" }, committer = new { login = "committer2" } },
            new { sha = "553c2077f0edc3d5dc5d17262f6aa498e69d6f8e", author = new { login = "user3" }, committer = new { login = "committer3" } }
        };


        _httpMessageHandlerMock.SetupRequest(HttpMethod.Get, url)
            .ReturnsResponse(HttpStatusCode.OK, commits);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var contributors = result.Value;
        Assert.Contains("user1", contributors);
        Assert.Contains("user2", contributors);
        Assert.Contains("user3", contributors);        
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNoCommitsFound()
    {
        // Arrange
        var query = new GetContributorsQuery { Owner = "testOwner", Repo = "testRepo" };
        var url = $"https://api.github.com/repos/{query.Owner}/{query.Repo}/commits?per_page=100";
        
        _httpMessageHandlerMock.SetupRequest(HttpMethod.Get, url)
            .ReturnsResponse(HttpStatusCode.OK, new List<dynamic>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("No commits found", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenApiResponseIsNull()
    {
        // Arrange
        var query = new GetContributorsQuery { Owner = "testOwner", Repo = "testRepo" };
        var url = $"https://api.github.com/repos/{query.Owner}/{query.Repo}/commits?per_page=100";

        // Mock HTTP response (200 OK with empty content - not null)
        _httpMessageHandlerMock.SetupRequest(HttpMethod.Get, url)
            .ReturnsResponse(HttpStatusCode.OK, "{}");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);        
    }

}

public static class HttpMessageHandlerExtensions
{
    public static Mock<HttpMessageHandler> SetupRequest(this Mock<HttpMessageHandler> handlerMock, HttpMethod method, string url)
    {
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(req => req.Method == method && req.RequestUri.ToString() == url), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage())
            .Verifiable();

        return handlerMock;
    }

    public static Mock<HttpMessageHandler> ReturnsResponse(this Mock<HttpMessageHandler> handlerMock, HttpStatusCode statusCode, object content = null)
    {
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content != null ? Newtonsoft.Json.JsonConvert.SerializeObject(content) : "")
            });

        return handlerMock;
    }
}
