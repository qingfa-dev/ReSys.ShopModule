using BuildingBlock.Monad.Errors;
using BuildingBlock.Monad.Results;

namespace BuildingBlock.Monad.UnitTest.Results;

/// <summary>Tests for <see cref="ResultException"/>.</summary>
[Trait("Category", "Unit")]
public class ResultExceptionSpec
{
    #region Properties

    /// <summary>Exception should expose the wrapped result and its errors.</summary>
    [Fact]
    public void Exception_Should_Expose_The_Wrapped_Result()
    {
        // Arrange
        var first = Error.InternalServerError("test.first", "first failed");
        var second = Error.InternalServerError("test.second", "second failed");
        var result = Result.Fail(first, second);

        // Act
        var exception = new ResultException(result);

        // Assert
        exception.Result.ShouldBeSameAs(result);
        exception.Errors.ShouldBe(result.Errors);
        exception.StatusCode.ShouldBe(result.StatusCode);
        exception.Message.ShouldContain(first.ToString());
        exception.Message.ShouldContain(second.ToString());
        exception.Message.ShouldContain(Environment.NewLine);
    }

    #endregion

    #region Message

    /// <summary>Exception without errors should describe the status.</summary>
    [Fact]
    public void Exception_Without_Errors_Should_Describe_The_Status()
    {
        // Act
        var exception = new ResultException(Result.Ok());

        // Assert
        exception.Message.ShouldBe(
            $"Result failed with status {ResultConstant.StatusCode.Ok}.");
    }

    #endregion
}
