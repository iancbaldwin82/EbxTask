using Application.Contributors.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/{owner}/{repo}/contributors")]
[ApiController]
public class ContributorsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetContributors(string owner, string repo)
    {
        var result = await mediator.Send(new GetContributorsQuery { Owner = owner, Repo = repo });

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}
