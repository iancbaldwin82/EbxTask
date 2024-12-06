using EbxTask.Application.Models;
using MediatR;
using System.Text.Json;

namespace Application.Contributors.Queries;

public class GetContributorsQueryHandler : IRequestHandler<GetContributorsQuery, Result<List<string>>>
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GetContributorsQueryHandler> _logger;

    public GetContributorsQueryHandler(HttpClient httpClient, ILogger<GetContributorsQueryHandler> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<List<string>>> Handle(GetContributorsQuery request, CancellationToken cancellationToken)
    {
        var url = $"https://api.github.com/repos/{request.Owner}/{request.Repo}/commits?per_page=100";
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        httpRequest.Headers.Add("User-Agent", "EbxTest");

        _logger.LogInformation("Sending request to GitHub API: {Url}", url);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        _logger.LogInformation($"Received response with status code: {response.StatusCode} for URL: {url}");


        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning($"No commits found in the response for repository {request.Owner}/{request.Repo}");
            return Result<List<string>>.Failure($"Repository '{request.Owner}/{request.Repo}' not found");
        }

        if (!response.IsSuccessStatusCode)
        {
            return Result<List<string>>.Failure($"Error fetching contributors: {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrEmpty(content))
        {
            return Result<List<string>>.Failure("No commits found");
        }

        try
        {
            var commits = JsonSerializer.Deserialize<List<CommitResponse>>(content);
            if (commits == null || commits.Count == 0)
            {
                _logger.LogWarning($"No commits found after deserialization for repository {request.Owner}/{request.Repo}");
                return Result<List<string>>.Failure("No commits found");
            }

            var contributors = commits
                .SelectMany(commit =>
                    new[] { commit.Author?.Login, commit.Committer?.Login })
                .Where(login => !string.IsNullOrEmpty(login))
                .Distinct()
                .Cast<string>()
                .ToList();

            _logger.LogInformation($"Found {contributors.Count} distinct contributors");
            return Result<List<string>>.Success(contributors);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, $"Error deserializing response content for repository {request.Owner}, {request.Repo}");
            return Result<List<string>>.Failure($"Error deserialising response: {ex.Message}");
        }        
    }
}
