using BuildingBlock.Monad.Errors;
using BuildingBlock.Monad.Metadata;
using BuildingBlock.Monad.Results;

namespace BuildingBlock.Monad.UnitTest.Results;

/// <summary>Tests for the non-generic <see cref="Result"/> record.</summary>
[Trait("Category", "Unit")]
public class ResultSpec
{
    #region Constructor

    /// <summary>Constructor should default status by state when status is null.</summary>
    [Theory]
    [InlineData(true, ResultConstant.StatusCode.Ok)]
    [InlineData(false, ResultConstant.StatusCode.InternalServerError)]
    public void Constructor_With_Null_Status_Should_Default_By_State(bool isSuccess, int expectedStatus)
    {
        // Arrange
        List<Error>? errors = isSuccess
            ? null
            : [new Error("test.generic", "A failure without an explicit status")];

        // Act
        var result = new Result(isSuccess, errors!, statusCode: null);

        // Assert
        result.IsSuccess.ShouldBe(isSuccess);
        result.StatusCode.ShouldBe(expectedStatus);
        if (isSuccess)
        {
            result.Errors.ShouldBeEmpty();
        }
        else
        {
            result.Errors.ShouldBe(errors);
        }
    }

    #endregion

    #region Equality

    /// <summary>Results with same state should be equal.</summary>
    [Fact]
    public void Result_Equality_Same_State_Should_Be_Equal()
    {
        // Arrange
        var error = ResultStub.NotFoundError();
        var left = Result.Fail(error);
        var right = Result.Fail(error);

        // Act & Assert
        left.Equals(right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        (left != right).ShouldBeFalse();
        left.Equals((Result?)null).ShouldBeFalse();
    }

    /// <summary>Results with different state should not be equal.</summary>
    [Fact]
    public void Result_Equality_When_State_Differs_Should_Not_Be_Equal()
    {
        // Arrange
        var failure = Result.Fail(ResultStub.NotFoundError());
        var single = Result.Fail(ResultStub.ConflictError());
        var doubled = Result.Fail(ResultStub.ConflictError(), ResultStub.ConflictError());

        // Act & Assert
        failure.Equals(Result.Ok()).ShouldBeFalse();
        failure.Equals(single).ShouldBeFalse();
        single.Equals(doubled).ShouldBeFalse();
    }

    /// <summary>Equality should exclude metadata but include status code.</summary>
    [Fact]
    public void Result_Equality_Should_Exclude_Metadata_And_Include_Status()
    {
        // Arrange
        var error = ResultStub.NotFoundError();
        var left = Result.Fail(error)
            .WithMetadata("k", "v");
        var right = Result.Fail(error);
        var otherStatus = Result.Fail(
            new List<Error> { error },
            ResultConstant.StatusCode.InternalServerError);

        // Act & Assert
        left.Metadata.ShouldNotBe(right.Metadata);
        left.Equals(right).ShouldBeTrue();
        left.Equals(otherStatus).ShouldBeFalse();
    }

    /// <summary>Same state should produce the same hash code.</summary>
    [Fact]
    public void Result_GetHashCode_Same_State_Should_Match()
    {
        // Arrange
        var error = ResultStub.NotFoundError();
        var left = Result.Fail(error);
        var right = Result.Fail(error);

        // Act & Assert
        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    #endregion

    #region CopyWith

    /// <summary>CopyWith with no changes should preserve state.</summary>
    [Fact]
    public void CopyWith_Without_Changes_Should_Preserve_State()
    {
        // Arrange
        var original = Result.Ok();

        // Act
        var copy = original.CopyWith();

        // Assert
        copy.IsSuccess.ShouldBeTrue();
        copy.StatusCode.ShouldBe(original.StatusCode);
        copy.Metadata.ShouldNotBeSameAs(original.Metadata);
    }

    /// <summary>CopyWith with explicit status should use the provided status.</summary>
    [Fact]
    public void CopyWith_With_Explicit_Status_Should_Use_Provided_Status()
    {
        // Arrange
        var original = Result.Ok();

        // Act
        var copy = original.CopyWith(statusCode: ResultConstant.StatusCode.Created);

        // Assert
        copy.StatusCode.ShouldBe(ResultConstant.StatusCode.Created);
    }

    /// <summary>CopyWith to success on failure should reset status to 200.</summary>
    [Fact]
    public void CopyWith_To_Success_On_Failure_Should_Reset_Status_To_Ok()
    {
        // Arrange
        var failure = Result.Fail(ResultStub.NotFoundError());

        // Act
        var copy = failure.CopyWith(isSuccess: true);

        // Assert
        copy.IsSuccess.ShouldBeTrue();
        copy.Errors.ShouldBeEmpty();
        copy.StatusCode.ShouldBe(ResultConstant.StatusCode.Ok);
    }

    /// <summary>CopyWith errors on failure should re-resolve status from new errors.</summary>
    [Fact]
    public void CopyWith_Errors_On_Failure_Should_ReResolve_Status_From_New_Errors()
    {
        // Arrange
        var failure = Result.Fail(ResultStub.NotFoundError());

        // Act
        var copy = failure.CopyWith(errors: new List<Error> { ResultStub.ConflictError() });

        // Assert
        copy.StatusCode.ShouldBe(ResultConstant.StatusCode.Conflict);
        copy.Errors[0].Code.ShouldBe("test.conflict");
    }

    /// <summary>CopyWith errors on success should preserve status.</summary>
    [Fact]
    public void CopyWith_Errors_On_Success_Should_Preserve_Status()
    {
        // Act
        var copy = Result.Ok().CopyWith(errors: new List<Error> { ResultStub.NotFoundError() });

        // Assert
        copy.IsSuccess.ShouldBeTrue();
        copy.StatusCode.ShouldBe(ResultConstant.StatusCode.Ok);
        copy.Errors.Count.ShouldBe(1);
        copy.Errors[0].Code.ShouldBe("test.not_found");
    }

    /// <summary>CopyWith with metadata should use the provided bag.</summary>
    [Fact]
    public void CopyWith_With_Metadata_Should_Use_Provided_Bag()
    {
        // Arrange
        var original = Result.Ok().WithMetadata("source", "original");
        var replacement = MetadataDictionary.Create().With("source", "replacement");

        // Act
        var copy = original.CopyWith(metadata: replacement);

        // Assert
        copy.Metadata.ShouldNotBeSameAs(replacement);
        copy.Metadata["source"].ShouldBe("replacement");
    }

    /// <summary>CopyWith should copy failure state and clone errors and metadata.</summary>
    [Fact]
    public void CopyWith_Should_Copy_Failure_State_And_Clone_Errors_And_Metadata()
    {
        // Arrange
        var original = Result.Ok().WithMetadata("source", "original");
        var errors = new List<Error> { ResultStub.NotFoundError() };

        // Act
        var copy = original.CopyWith(isSuccess: false, errors: errors);

        // Assert
        copy.IsFailure.ShouldBeTrue();
        copy.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
        copy.Errors.ShouldBe(errors);
        copy.Errors.ShouldNotBeSameAs(errors);
        copy.Metadata.ShouldNotBeSameAs(original.Metadata);
        copy.Metadata["source"].ShouldBe("original");

        errors[0] = ResultStub.ConflictError();
        copy.Errors[0].Code.ShouldBe("test.not_found");
    }

    #endregion

    #region State

    /// <summary>Result should expose state and render success and failure.</summary>
    [Fact]
    public void Result_Should_Expose_State_And_Render_Success_And_Failure()
    {
        // Arrange
        var success = Result.Ok();
        var failure = Result.Fail(ResultStub.NotFoundError());

        // Act & Assert
        success.HasValue.ShouldBeTrue();
        success.IsSuccess.ShouldBeTrue();
        success.IsFailure.ShouldBeFalse();
        success.ToString().ShouldBe("Success (200)");

        failure.HasValue.ShouldBeFalse();
        failure.IsSuccess.ShouldBeFalse();
        failure.IsFailure.ShouldBeTrue();
        failure.ToString().ShouldContain("test.not_found");
    }

    #endregion
}
