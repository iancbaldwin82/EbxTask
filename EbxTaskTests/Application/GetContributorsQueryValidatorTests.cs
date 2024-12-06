using Application.Contributors.Queries;
using FluentValidation.TestHelper;

namespace EbxTaskTests.Application.Validators;

public class GetContributorsQueryValidatorTests
{
    private readonly GetContributorsQueryValidator _validator;

    public GetContributorsQueryValidatorTests()
    {
        _validator = new GetContributorsQueryValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Owner_Is_Empty()
    {
        // Arrange
        var query = new GetContributorsQuery
        {
            Owner = string.Empty,
            Repo = "testRepo"
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Owner)
            .WithErrorMessage("Owner is required");
    }

    [Fact]
    public void Should_Have_Error_When_Repo_Is_Empty()
    {
        // Arrange
        var query = new GetContributorsQuery
        {
            Owner = "testOwner",
            Repo = string.Empty
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Repo)
            .WithErrorMessage("Repository is required");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Valid_Query()
    {
        // Arrange
        var query = new GetContributorsQuery
        {
            Owner = "some-owner",
            Repo = "some-repo"
        };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Owner);
        result.ShouldNotHaveValidationErrorFor(x => x.Repo);
    }
}
