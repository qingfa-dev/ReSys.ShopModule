using BuildingBlock.Monad.Errors;

namespace BuildingBlock.Monad.UnitTest.Errors;

/// <summary>Tests for <see cref="ErrorGuard"/> validation methods.</summary>
/// <remarks>
/// Covers null/whitespace/length validation for code, message, and status fields.
/// </remarks>
[Trait("Category", "Unit")]
public class ErrorGuardSpec
{
    #region ValidateCode

    [Fact]
    public void ValidateCode_When_Null_Should_Throw_ArgumentNullException()
    {
        // Arrange
        // Act
        var act = () => ErrorGuard.ValidateCode(null);

        // Assert
        var ex = Should.Throw<ArgumentNullException>(act);
        ex.ParamName.ShouldBe("code");
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Code.NullOrWhitespace.Code);
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Code.NullOrWhitespace.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ValidateCode_When_Whitespace_Should_Throw_ArgumentException(string? code)
    {
        // Arrange
        // Act
        var act = () => ErrorGuard.ValidateCode(code);

        // Assert
        var ex = Should.Throw<ArgumentException>(act);
        ex.ParamName.ShouldBe("code");
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Code.NullOrWhitespace.Code);
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Code.NullOrWhitespace.Message);
    }

    [Fact]
    public void ValidateCode_When_At_Max_Length_Should_Not_Throw()
    {
        // Arrange
        var code = new string('c', ErrorConstant.Constraint.Code.MaxLength);

        // Act
        var act = () => ErrorGuard.ValidateCode(code);

        // Assert
        Should.NotThrow(act);
    }

    [Fact]
    public void ValidateCode_When_Exceeds_Max_Length_Should_Throw_Range_With_Formatted_Pattern()
    {
        // Arrange
        var code = new string('c', ErrorConstant.Constraint.Code.MaxLength + 1);

        // Act
        var act = () => ErrorGuard.ValidateCode(code);

        // Assert
        var ex = Should.Throw<ArgumentOutOfRangeException>(act);
        ex.ParamName.ShouldBe("code");
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Code.ExceedsMaxLength.Code);
        ex.Message.ShouldContain("'256'");
    }

    #endregion

    #region ValidateMessage

    [Fact]
    public void ValidateMessage_When_Null_Should_Throw_ArgumentNullException()
    {
        // Arrange
        // Act
        var act = () => ErrorGuard.ValidateMessage(null);

        // Assert
        var ex = Should.Throw<ArgumentNullException>(act);
        ex.ParamName.ShouldBe("message");
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Message.NullOrWhitespace.Code);
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Message.NullOrWhitespace.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ValidateMessage_When_Whitespace_Should_Throw_ArgumentException(string? message)
    {
        // Arrange
        // Act
        var act = () => ErrorGuard.ValidateMessage(message);

        // Assert
        var ex = Should.Throw<ArgumentException>(act);
        ex.ParamName.ShouldBe("message");
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Message.NullOrWhitespace.Code);
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Message.NullOrWhitespace.Message);
    }

    [Fact]
    public void ValidateMessage_When_At_Max_Length_Should_Not_Throw()
    {
        // Arrange
        var message = new string('m', ErrorConstant.Constraint.Message.MaxLength);

        // Act
        var act = () => ErrorGuard.ValidateMessage(message);

        // Assert
        Should.NotThrow(act);
    }

    [Fact]
    public void ValidateMessage_When_Exceeds_Max_Length_Should_Throw_Range_With_Formatted_Pattern()
    {
        // Arrange
        var message = new string('m', ErrorConstant.Constraint.Message.MaxLength + 1);

        // Act
        var act = () => ErrorGuard.ValidateMessage(message);

        // Assert
        var ex = Should.Throw<ArgumentOutOfRangeException>(act);
        ex.ParamName.ShouldBe("message");
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Message.ExceedsMaxLength.Code);
        ex.Message.ShouldContain("'1024'");
    }

    #endregion

    #region ValidateStatus

    [Theory]
    [InlineData(99)]
    [InlineData(600)]
    public void ValidateStatus_When_Out_Of_Range_Should_Throw_Range_With_Formatted_Pattern(int status)
    {
        // Arrange
        // Act
        var act = () => ErrorGuard.ValidateStatus(status);

        // Assert
        var ex = Should.Throw<ArgumentOutOfRangeException>(act);
        ex.ParamName.ShouldBe("status");
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Status.OutOfRange.Code);
        ex.Message.ShouldContain("100 - 599");
    }

    [Theory]
    [InlineData(100)]
    [InlineData(599)]
    public void ValidateStatus_When_In_Range_Should_Not_Throw(int status)
    {
        // Arrange
        // Act
        var act = () => ErrorGuard.ValidateStatus(status);

        // Assert
        Should.NotThrow(act);
    }

    #endregion

    #region ValidateInstance

    [Fact]
    public void ValidateInstance_When_Null_Should_Not_Throw()
    {
        // Arrange
        // Act
        var act = () => ErrorGuard.ValidateInstance(null);

        // Assert
        Should.NotThrow(act);
    }

    [Fact]
    public void ValidateInstance_When_At_Max_Length_Should_Not_Throw()
    {
        // Arrange
        var instance = new string('i', ErrorConstant.Constraint.Instance.MaxLength);

        // Act
        var act = () => ErrorGuard.ValidateInstance(instance);

        // Assert
        Should.NotThrow(act);
    }

    [Fact]
    public void ValidateInstance_When_Exceeds_Max_Length_Should_Throw_Range_With_Formatted_Pattern()
    {
        // Arrange
        var instance = new string('i', ErrorConstant.Constraint.Instance.MaxLength + 1);

        // Act
        var act = () => ErrorGuard.ValidateInstance(instance);

        // Assert
        var ex = Should.Throw<ArgumentOutOfRangeException>(act);
        ex.ParamName.ShouldBe("instance");
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Instance.ExceedsMaxLength.Code);
        ex.Message.ShouldContain("'256'");
    }

    #endregion
}