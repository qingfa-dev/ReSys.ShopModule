using BuildingBlock.Monad.Errors;
using BuildingBlock.Monad.Results;

namespace BuildingBlock.Monad.UnitTest.Results;

/// <summary>Tests for <see cref="IResult{TError}"/> and <see cref="IResult{TValue,TError}"/> contracts.</summary>
[Trait("Category", "Unit")]
public class ResultInterfaceSpec
{
    #region Non-generic

    /// <summary>Non-generic Result should implement IResult.</summary>
    [Fact]
    public void NonGeneric_Result_Should_Implement_IResult()
    {
        // Arrange
        IResult<Error> result = Result.Ok();

        // Act & Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.HasValue.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        result.StatusCode.ShouldBe(ResultConstant.StatusCode.Ok);
    }

    #endregion

    #region Generic

    /// <summary>Generic Result should implement IResult and IResult of value.</summary>
    [Theory]
    [InlineData(42)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Generic_Result_Should_Implement_IResult_And_IResultOfValue(int expectedValue)
    {
        // Arrange
        IResult<int, Error> result = Result<int>.Ok(expectedValue);

        // Act & Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedValue);
        result.TryGetValue(out var value).ShouldBeTrue();
        value.ShouldBe(expectedValue);
    }

    /// <summary>Generic failure should expose failed state through IResult contract.</summary>
    [Fact]
    public void Generic_Failure_Should_Expose_Failed_State_Through_IResult_Contract()
    {
        // Arrange
        IResult<int, Error> result = Result<int>.Fail(ResultStub.NotFoundError());

        // Act & Assert
        result.IsFailure.ShouldBeTrue();
        result.HasValue.ShouldBeFalse();
        result.TryGetValue(out var value).ShouldBeFalse();
        value.ShouldBe(default);
        result.Errors.ShouldHaveSingleItem();
    }

    #endregion
}
