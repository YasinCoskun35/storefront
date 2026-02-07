using FluentAssertions;
using Storefront.SharedKernel;

namespace Storefront.UnitTests.SharedKernel;

public class ResultTests
{
    [Fact]
    public void Success_Should_Create_Successful_Result()
    {
        // Arrange
        var value = "test value";

        // Act
        var result = Result<string>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Failure_Should_Create_Failed_Result()
    {
        // Arrange
        var error = Error.Validation("Test.Error", "Test error message");

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Accessing_Value_On_Failed_Result_Should_Throw()
    {
        // Arrange
        var error = Error.NotFound("Test.NotFound", "Item not found");
        var result = Result<string>.Failure(error);

        // Act
        Action act = () => { var value = result.Value; };

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*failure*");
    }

    [Fact]
    public void Error_NotFound_Should_Have_Correct_Type()
    {
        // Arrange & Act
        var error = Error.NotFound("Item.NotFound", "Item not found");

        // Assert
        error.Type.Should().Be("NotFound");
        error.Code.Should().Be("Item.NotFound");
        error.Message.Should().Be("Item not found");
    }

    [Fact]
    public void Error_Validation_Should_Have_Correct_Type()
    {
        // Arrange & Act
        var error = Error.Validation("Validation.Failed", "Validation failed");

        // Assert
        error.Type.Should().Be("Validation");
        error.Code.Should().Be("Validation.Failed");
    }

    [Fact]
    public void Error_Conflict_Should_Have_Correct_Type()
    {
        // Arrange & Act
        var error = Error.Conflict("Item.Exists", "Item already exists");

        // Assert
        error.Type.Should().Be("Conflict");
        error.Code.Should().Be("Item.Exists");
    }
}

