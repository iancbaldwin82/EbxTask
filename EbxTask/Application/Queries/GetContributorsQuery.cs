using EbxTask.Application.Models;
using MediatR;

namespace Application.Contributors.Queries;

public class GetContributorsQuery : IRequest<Result<List<string>>>
{
    public required string Owner { get; set; }
    public required string Repo { get; set; }
}
