using BuildingBlock.Monad.Errors;
using BuildingBlock.Monad.Metadata;
using BuildingBlock.Monad.Results;

namespace BuildingBlock.Monad.UnitTest.Results;

/// <summary>Tests for the generic <see cref="Result{TValue}"/> record.</summary>
[Trait("Category", "Unit")]
public class ResultGenericSpec
{
    #region CopyWith

    /// <summary>CopyWith without changes should preserve value and status.</summary>
    [Fact]
    public void CopyWith_Without_Changes_Should_Preserve_Value_And_Status()
    {
        // Arrange
        var original = Result<int>.Ok(5);

        // Act
        var copy = original.CopyWith();

        // Assert
        copy.Value.ShouldBe(5);
        copy.StatusCode.ShouldBe(ResultConstant.StatusCode.Ok);
        copy.Metadata.ShouldNotBeSameAs(original.Metadata);
    }

    /// <summary>CopyWith with explicit status should use the provided status.</summary>
    [Fact]
    public void CopyWith_With_Explicit_Status_Should_Use_Provided_Status()
    {
        // Act
        var copy = Result<int>.Ok(5).CopyWith(statusCode: ResultConstant.StatusCode.Created);

        // Assert
        copy.StatusCode.ShouldBe(ResultConstant.StatusCode.Created);
    }

    /// <summary>CopyWith errors on failure should keep status and use new errors.</summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CopyWith_Errors_On_Failure_Should_Preserve_Status_And_Use_New_Errors(bool withValue)
    {
        // Arrange
        var failure = Result<int>.Fail(ResultStub.NotFoundError());
        var errors = new List<Error> { ResultStub.ConflictError() };

        // Act
        var copy = withValue
            ? failure.CopyWith(value: 0, errors: errors)
            : failure.CopyWith(errors: errors);

        // Assert
        copy.IsFailure.ShouldBeTrue();
        copy.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
        copy.Errors[0].Code.ShouldBe("test.conflict");
    }

    /// <summary>CopyWith value with explicit status and metadata should apply them.</summary>
    [Fact]
    public void CopyWith_Value_With_Explicit_Status_And_Metadata_Should_Apply_Them()
    {
        // Arrange
        var original = Result<int>.Ok(5);
        var replacement = MetadataDictionary.Create().With("k", "v");

        // Act
        var copy = original.CopyWith(
            value: 7,
            statusCode: ResultConstant.StatusCode.Created,
            metadata: replacement);

        // Assert
        copy.Value.ShouldBe(7);
        copy.StatusCode.ShouldBe(ResultConstant.StatusCode.Created);
        copy.Metadata.ShouldNotBeSameAs(replacement);
        copy.Metadata["k"].ShouldBe("v");
    }

    /// <summary>CopyWith to failure should use new errors and keep the value unless one is provided.</summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CopyWith_To_Failure_Should_Use_Errors_And_Keep_Value_When_Not_Provided(bool withValue)
    {
        // Arrange
        var original = Result<int>.Ok(5);
        var errors = new List<Error> { ResultStub.NotFoundError() };

        // Act
        var copy = withValue
            ? original.CopyWith(value: 0, isSuccess: false, errors: errors)
            : original.CopyWith(isSuccess: false, errors: errors);

        // Assert
        copy.IsFailure.ShouldBeTrue();
        copy.Value.ShouldBe(withValue ? default : 5);
        copy.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
        copy.TryGetValue(out _).ShouldBeFalse();
    }

    /// <summary>CopyWith errors on success should preserve status and value.</summary>
    [Theory]
    [InlineData(true, 7)]
    [InlineData(false, 5)]
    public void CopyWith_Errors_On_Success_Should_Preserve_Status_And_Value(bool withValue, int expectedValue)
    {
        // Arrange
        var success = Result<int>.Ok(5);
        var errors = new List<Error> { ResultStub.NotFoundError() };

        // Act
        var copy = withValue
            ? success.CopyWith(value: 7, errors: errors)
            : success.CopyWith(errors: errors);

        // Assert
        copy.IsSuccess.ShouldBeTrue();
        copy.Value.ShouldBe(expectedValue);
        copy.StatusCode.ShouldBe(ResultConstant.StatusCode.Ok);
        copy.Errors.Count.ShouldBe(1);
        copy.Errors[0].Code.ShouldBe("test.not_found");
    }

    /// <summary>CopyWith value on failure without errors should preserve errors and status.</summary>
    [Fact]
    public void CopyWith_Value_On_Failure_Without_Errors_Should_Preserve_Errors_And_Status()
    {
        // Arrange
        var failure = Result<int>.Fail(ResultStub.NotFoundError());

        // Act
        var copy = failure.CopyWith(value: 0);

        // Assert
        copy.IsFailure.ShouldBeTrue();
        copy.Value.ShouldBe(default);
        copy.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
        copy.Errors.Count.ShouldBe(1);
        copy.Errors[0].Code.ShouldBe("test.not_found");
    }

    /// <summary>CopyWith with metadata should clone the provided bag.</summary>
    [Fact]
    public void CopyWith_With_Metadata_Should_Clone_Provided_Bag()
    {
        // Arrange
        var replacement = MetadataDictionary.Create().With("k", "v");

        // Act
        var copy = Result<int>.Ok(5).CopyWith(metadata: replacement);

        // Assert
        copy.Metadata.ShouldNotBeSameAs(replacement);
        copy.Metadata["k"].ShouldBe("v");
    }

    /// <summary>CopyWith should replace value and clone metadata.</summary>
    [Fact]
    public void CopyWith_Should_Replace_Value_And_Clone_Metadata()
    {
        // Arrange
        var original = Result<int>.Ok(5).WithMetadata("source", "original");

        // Act
        var copy = original.CopyWith(value: 10);

        // Assert
        copy.Value.ShouldBe(10);
        copy.StatusCode.ShouldBe(original.StatusCode);
        copy.Metadata.ShouldNotBeSameAs(original.Metadata);
        copy.Metadata["source"].ShouldBe("original");
    }

    /// <summary>CopyWith to failure without a value should retain the value but block TryGetValue.</summary>
    [Fact]
    public void CopyWith_When_Changing_To_Failure_Should_Retain_Value_And_Block_TryGetValue()
    {
        // Arrange
        var original = Result<int>.Ok(5);
        var errors = new List<Error> { ResultStub.NotFoundError() };

        // Act
        var copy = original.CopyWith(isSuccess: false, errors: errors);

        // Assert
        copy.IsFailure.ShouldBeTrue();
        copy.Value.ShouldBe(5);
        copy.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
        copy.TryGetValue(out _).ShouldBeFalse();
    }

    /// <summary>CopyWith should require replacement value when changing failure to success.</summary>
    [Fact]
    public void CopyWith_Should_Require_Replacement_Value_When_Changing_Failure_To_Success()
    {
        // Arrange
        var failure = Result<int>.Fail(ResultStub.NotFoundError());

        // Act
        var act = () => failure.CopyWith(isSuccess: true);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    /// <summary>CopyWith with replacement value should allow failure to success transition.</summary>
    [Fact]
    public void CopyWith_With_Replacement_Value_Should_Allow_Failure_To_Success_Transition()
    {
        // Arrange
        var failure = Result<int>.Fail(ResultStub.NotFoundError());

        // Act
        var copy = failure.CopyWith(value: 42, isSuccess: true);

        // Assert
        copy.IsSuccess.ShouldBeTrue();
        copy.Value.ShouldBe(42);
        copy.Errors.ShouldBeEmpty();
        copy.StatusCode.ShouldBe(ResultConstant.StatusCode.Ok);
    }

    #endregion

    #region Equality

    /// <summary>Generic result equality should compare state and value.</summary>
    [Fact]
    public void Result_Generic_Equality_Should_Compare_State_And_Value()
    {
        // Arrange
        var left = Result<int>.Ok(5);
        var right = Result<int>.Ok(5);

        // Act & Assert
        left.Equals(right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        left.Equals(Result<int>.Ok(6)).ShouldBeFalse();
        left.Equals(Result<int>.Fail(ResultStub.NotFoundError())).ShouldBeFalse();
        left.Equals((Result<int>?)null).ShouldBeFalse();
    }

    /// <summary>Generic result GetHashCode should match for same state.</summary>
    [Fact]
    public void Result_Generic_GetHashCode_Same_State_Should_Match()
    {
        // Arrange
        var left = Result<int>.Fail(ResultStub.NotFoundError());
        var right = Result<int>.Fail(ResultStub.NotFoundError());

        // Act & Assert
        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    #endregion

    #region Value Access

    /// <summary>TryGetValue and ToString should reflect result state.</summary>
    [Fact]
    public void TryGetValue_And_ToString_Should_Reflect_Result_State()
    {
        // Arrange
        var success = Result<int>.Ok(42);
        var failure = Result<int>.Fail(ResultStub.NotFoundError());

        // Act & Assert
        success.TryGetValue(out var value).ShouldBeTrue();
        value.ShouldBe(42);
        success.ToString().ShouldBe("Success (200): 42");

        failure.TryGetValue(out _).ShouldBeFalse();
        failure.ToString().ShouldContain("Failure (404):");
        failure.ToString().ShouldContain("test.not_found");
    }

    #endregion

    #region Constructor

    /// <summary>Constructor should reject a success without a value.</summary>
    [Fact]
    public void Constructor_Should_Reject_A_Success_Without_A_Value()
    {
        // Act
        var act = () => new Result<string>(isSuccess: true, value: null!);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    #endregion
}