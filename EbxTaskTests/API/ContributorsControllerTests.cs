using API.Controllers;
using Application.Contributors.Queries;
using EbxTask.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EbxTask.Tests.API.Controllers;

public class ContributorsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ContributorsController _controller;

    public ContributorsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ContributorsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetContributors_ShouldReturnOk_WhenContributorsAreFound()
    {
        // Arrange
        var query = new GetContributorsQuery { Owner = "testOwner", Repo = "testRepo" };
        var contributors = new List<string> { "user1", "user2" };
        var result = Result<List<string>>.Success(contributors);
        _mediatorMock.Setup(mediator => mediator.Send(It.IsAny<GetContributorsQuery>(), default))
                     .ReturnsAsync(result);

        // Act
        var response = await _controller.GetContributors("testOwner", "testRepo");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response);
        var returnValue = Assert.IsType<List<string>>(okResult.Value);
        Assert.Contains("user1", returnValue);
        Assert.Contains("user2", returnValue);
    }

    [Fact]
    public async Task GetContributors_ShouldReturnNotFound_WhenRepositoryIsNotFound()
    {
        // Arrange
        var query = new GetContributorsQuery { Owner = "testOwner", Repo = "testRepo" };
        var result = Result<List<string>>.Failure("Repository 'testOwner/testRepo' not found.");
        _mediatorMock.Setup(mediator => mediator.Send(It.IsAny<GetContributorsQuery>(), default))
                     .ReturnsAsync(result);

        // Act
        var response = await _controller.GetContributors("testOwner", "testRepo");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetContributors_ShouldReturnNotFound_WhenNoCommitsFound()
    {
        // Arrange
        var query = new GetContributorsQuery { Owner = "testOwner", Repo = "testRepo" };
        var result = Result<List<string>>.Failure("No commits found");
        _mediatorMock.Setup(mediator => mediator.Send(It.IsAny<GetContributorsQuery>(), default))
                     .ReturnsAsync(result);

        // Act
        var response = await _controller.GetContributors("testOwner", "testRepo");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetContributors_ShouldReturnNotFound_WhenApiCallFails()
    {
        // Arrange
        var query = new GetContributorsQuery { Owner = "testOwner", Repo = "testRepo" };
        var result = Result<List<string>>.Failure("Error fetching contributors: Internal Server Error");
        _mediatorMock.Setup(mediator => mediator.Send(It.IsAny<GetContributorsQuery>(), default))
                     .ReturnsAsync(result);

        // Act
        var response = await _controller.GetContributors("testOwner", "testRepo");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response);        
        Assert.Equal(404, notFoundResult.StatusCode);
    }
}
