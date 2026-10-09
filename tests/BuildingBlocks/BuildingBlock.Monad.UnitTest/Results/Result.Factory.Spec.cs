using BuildingBlock.Monad.Errors;
using BuildingBlock.Monad.Results;

namespace BuildingBlock.Monad.UnitTest.Results;

/// <summary>Tests for <see cref="Result"/> factory methods.</summary>
[Trait("Category", "Unit")]
public class ResultFactorySpec
{
    #region Success

    /// <summary>Success factories should use their standard status.</summary>
    [Theory]
    [InlineData(200, 200)]
    [InlineData(201, 201)]
    [InlineData(202, 202)]
    [InlineData(204, 204)]
    public void Success_Factories_Should_Use_Their_Standard_Status(int expected, int actual)
    {
        // Act
        var result = actual switch
        {
            200 => Result.Ok(),
            201 => Result.Created(),
            202 => Result.Accepted(),
            204 => Result.NoContent(),
            _ => throw new ArgumentOutOfRangeException(nameof(actual)),
        };

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.StatusCode.ShouldBe(expected);
        result.Errors.ShouldBeEmpty();
    }

    /// <summary>Success with explicit status should use the provided status.</summary>
    [Fact]
    public void Success_With_Explicit_Status_Should_Use_Provided_Status()
    {
        // Act
        var result = Result.Success(ResultConstant.StatusCode.Accepted);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.StatusCode.ShouldBe(ResultConstant.StatusCode.Accepted);
    }

    #endregion

    #region Failure

    /// <summary>Failure without explicit status should resolve status from error.</summary>
    [Fact]
    public void Failure_Without_Explicit_Status_Should_Resolve_Status_From_Error()
    {
        // Arrange
        var error = ResultStub.NotFoundError();

        // Act
        var result = Result.Fail(error);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
        result.Errors.ShouldContain(error);
    }

    /// <summary>Failure with explicit status should use the provided status.</summary>
    [Fact]
    public void Failure_With_Explicit_Status_Should_Use_Provided_Status()
    {
        // Arrange
        var error = ResultStub.NotFoundError();

        // Act
        var result = Result.Fail(new List<Error> { error }, ResultConstant.StatusCode.InternalServerError);

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
                => () => { _ = Result.Success(ResultConstant.StatusCode.NotFound); },
            "failure-with-success-status"
                => () => { _ = Result.Fail(new List<Error> { ResultStub.NotFoundError() }, ResultConstant.StatusCode.Ok); },
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
        var resolved = Result.Failure(errors);
        var explicitStatus = Result.Failure(errors, ResultConstant.StatusCode.ServiceUnavailable);

        // Assert
        resolved.IsFailure.ShouldBeTrue();
        resolved.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
        explicitStatus.StatusCode.ShouldBe(ResultConstant.StatusCode.ServiceUnavailable);
        explicitStatus.Errors.ShouldBe(errors);
    }

    /// <summary>Fail with non-list enumerable should materialize errors.</summary>
    [Fact]
    public void Fail_With_Non_List_Enumerable_Should_Materialize_Errors()
    {
        // Act
        var result = Result.Fail(Enumerate(ResultStub.NotFoundError()).ToList());

        // Assert
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].Code.ShouldBe("test.not_found");
    }

    /// <summary>Failure when no error carries status should default to internal server error.</summary>
    [Fact]
    public void Failure_When_No_Error_Carries_Status_Should_Default_To_Internal_Server_Error()
    {
        // Arrange
        var critical = Error.Custom("test.critical", "critical", severity: ErrorSeverity.Critical);
        var info = Error.Custom("test.info", "info", severity: ErrorSeverity.Info);

        // Act
        var result = Result.Fail(critical, info);

        // Assert
        result.StatusCode.ShouldBe(ResultConstant.StatusCode.InternalServerError);
    }

/// <summary>Failure with mixed status errors should reject.</summary>
    [Fact]
    public void Failure_With_Mixed_Status_Errors_Should_Reject()
    {
        // Arrange
        var withStatus = ResultStub.NotFoundError();
        var criticalNoStatus = Error.Custom("test.critical", "critical", severity: ErrorSeverity.Critical);
        var infoNoStatus = Error.Custom("test.info", "info", severity: ErrorSeverity.Info);

        // Act
        var act = () => Result.Fail(withStatus, criticalNoStatus, infoNoStatus);

        // Assert
        var ex = Should.Throw<ArgumentException>(act);
        ex.Message.ShouldContain(ResultOutcome.Failure.Errors.MixedStatusCodes.Code);
    }

    #endregion

    #region Custom

    /// <summary>Custom without explicit status should resolve defaults.</summary>
    [Fact]
    public void Custom_Without_Explicit_Status_Should_Resolve_Defaults()
    {
        // Act
        var success = Result.Custom(isSuccess: true);

        // Assert
        success.IsSuccess.ShouldBeTrue();
        success.StatusCode.ShouldBe(ResultConstant.StatusCode.Ok);
        success.Errors.ShouldBeEmpty();
    }

    /// <summary>Custom failure without explicit status should resolve from errors.</summary>
    [Fact]
    public void Custom_Failure_Without_Explicit_Status_Should_Resolve_From_Errors()
    {
        // Act
        var result = Result.Custom(
            isSuccess: false,
            errors: new List<Error> { ResultStub.NotFoundError() });

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
    }

    /// <summary>Custom failure without errors should reject.</summary>
    [Fact]
    public void Custom_Failure_Without_Errors_Should_Reject()
    {
        // Act
        var act = () => Result.Custom(isSuccess: false);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    #endregion

    private static IEnumerable<Error> Enumerate(Error error)
    {
        yield return error;
    }
}