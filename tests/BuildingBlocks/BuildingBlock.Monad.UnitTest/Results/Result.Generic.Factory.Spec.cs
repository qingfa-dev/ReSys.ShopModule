using BuildingBlock.Monad.Errors;
using BuildingBlock.Monad.Results;

namespace BuildingBlock.Monad.UnitTest.Results;

/// <summary>Tests for <see cref="Result{TValue}"/> factory methods.</summary>
[Trait("Category", "Unit")]
public class ResultGenericFactorySpec
{
    #region Success

    /// <summary>Success factories should use their standard status.</summary>
    [Theory]
    [InlineData(ResultConstant.StatusCode.Ok, 0)]
    [InlineData(ResultConstant.StatusCode.Created, 1)]
    [InlineData(ResultConstant.StatusCode.Accepted, 2)]
    public void Success_Factories_Should_Use_Their_Standard_Status(int expected, int factory)
    {
        // Act
        var result = factory switch
        {
            0 => Result<int>.Ok(1),
            1 => Result<int>.Created(2),
            2 => Result<int>.Accepted(3),
            _ => throw new ArgumentOutOfRangeException(nameof(factory)),
        };

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.StatusCode.ShouldBe(expected);
    }

    /// <summary>Success with explicit status should use the provided status.</summary>
    [Fact]
    public void Success_With_Explicit_Status_Should_Use_Provided_Status()
    {
        // Act
        var result = Result<int>.Ok(1, ResultConstant.StatusCode.Accepted);

        // Assert
        result.StatusCode.ShouldBe(ResultConstant.StatusCode.Accepted);
        result.Value.ShouldBe(1);
    }

    #endregion

    #region Failure

    /// <summary>Failure without explicit status should resolve status from error.</summary>
    [Fact]
    public void Failure_Without_Explicit_Status_Should_Resolve_Status_From_Error()
    {
        // Arrange
        var result = Result<int>.Fail(ResultStub.NotFoundError());

        // Act & Assert
        result.IsFailure.ShouldBeTrue();
        result.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
        result.TryGetValue(out _).ShouldBeFalse();
    }

    /// <summary>Failure with explicit status should use the provided status.</summary>
    [Fact]
    public void Failure_With_Explicit_Status_Should_Use_Provided_Status()
    {
        // Arrange
        var result = Result<int>.Fail(
            new List<Error> { ResultStub.NotFoundError() },
            ResultConstant.StatusCode.InternalServerError);

        // Assert
        result.StatusCode.ShouldBe(ResultConstant.StatusCode.InternalServerError);
    }

    /// <summary>Factory validation should reject invalid input.</summary>
    [Theory]
    [InlineData("success-with-failure-status")]
    [InlineData("failure-with-success-status")]
    [InlineData("failure-with-empty-errors")]
    public void Factory_Validation_Should_Reject_Invalid_Input(string scenario)
    {
        // Act
        Action act = scenario switch
        {
            "success-with-failure-status"
                => () => { _ = Result<int>.Success(1, ResultConstant.StatusCode.NotFound); },
            "failure-with-success-status"
                => () => { _ = Result<int>.Fail(new List<Error> { ResultStub.NotFoundError() }, ResultConstant.StatusCode.Ok); },
            "failure-with-empty-errors"
                => () => { _ = Result.Fail(new List<Error>()); },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    /// <summary>Failure enumerable overloads should create failures.</summary>
    [Fact]
    public void Failure_Enumerable_Overloads_Should_Create_Failures()
    {
        // Arrange
        var errors = new List<Error> { ResultStub.NotFoundError() };

        // Act
        var resolved = Result<string>.Failure(errors);
        var explicitStatus = Result<string>.Failure(errors, ResultConstant.StatusCode.ServiceUnavailable);

        // Assert
        resolved.IsFailure.ShouldBeTrue();
        resolved.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
        explicitStatus.StatusCode.ShouldBe(ResultConstant.StatusCode.ServiceUnavailable);
        explicitStatus.Errors.ShouldBe(errors);
    }

    /// <summary>Generic fail with non-list enumerable should materialize errors.</summary>
    [Fact]
    public void Generic_Fail_With_Non_List_Enumerable_Should_Materialize_Errors()
    {
        // Act
        var result = Result<string>.Fail(Enumerate(ResultStub.NotFoundError()).ToList());

        // Assert
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].Code.ShouldBe("test.not_found");
    }

    #endregion

    #region Custom

    /// <summary>Custom factory without errors should keep success defaults.</summary>
    [Fact]
    public void Custom_Without_Errors_Should_Keep_Success_Defaults()
    {
        // Act
        var result = Result<int>.Custom(isSuccess: true, value: 42);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
        result.Errors.ShouldBeEmpty();
        result.StatusCode.ShouldBe(ResultConstant.StatusCode.Ok);
    }

    /// <summary>Custom factory with errors should forward status and materialize errors.</summary>
    [Fact]
    public void Custom_With_Errors_Should_Forward_Status_And_Materialize_Errors()
    {
        // Arrange
        var errors = new List<Error> { ResultStub.NotFoundError() };

        // Act
        var result = Result<int>.Custom(
            isSuccess: false,
            value: 0,
            errors: errors,
            statusCode: ResultConstant.StatusCode.NotFound);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeSameAs(errors);
        result.Errors.ShouldBe(errors);
        result.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
    }

    #endregion

    private static IEnumerable<Error> Enumerate(Error error)
    {
        yield return error;
    }
}