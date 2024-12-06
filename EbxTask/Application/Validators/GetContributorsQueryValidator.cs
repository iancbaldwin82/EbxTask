using FluentValidation;

namespace Application.Contributors.Queries;

public class GetContributorsQueryValidator : AbstractValidator<GetContributorsQuery>
{
    public GetContributorsQueryValidator()
    {
        RuleFor(q => q.Owner).NotEmpty().WithMessage("Owner is required");
        RuleFor(q => q.Repo).NotEmpty().WithMessage("Repository is required");
    }
}
