using BuildingBlock.Monad.Errors;

namespace BuildingBlock.Monad.UnitTest.Errors;

/// <summary>Tests for <see cref="Error"/> construction, guard rails, rendering, and value equality.</summary>
/// <remarks>
/// Covers default/optional construction, null/whitespace/length/status guard validation,
/// <c>Code:Message</c> rendering, and value equality (metadata excluded).
/// </remarks>
[Trait("Category", "Unit")]
public class ErrorSpec
{
    #region Construction

    [Fact]
    public void Error_When_Constructed_Should_Set_Contracted_Defaults()
    {
        // Arrange
        // Act
        var error = new Error("order.not_found", "Order was not found");

        // Assert
        error.Code.ShouldBe("order.not_found");
        error.Message.ShouldBe("Order was not found");
        error.Type.ShouldBeNull();
        error.Instance.ShouldBeNull();
        error.Status.ShouldBeNull();
        error.Severity.ShouldBe(ErrorSeverity.Error);
        error.Metadata.ShouldNotBeNull();
        error.Metadata.ShouldBeEmpty();
    }

    [Fact]
    public void Error_When_Constructed_With_Optionals_Should_Persist_All_Values()
    {
        // Arrange
        // Act
        var error = new Error(
            code: "order.invalid",
            message: "Order is invalid",
            status: 422,
            type: "https://errors.kernel/custom",
            instance: "/orders/42",
            severity: ErrorSeverity.Critical);

        // Assert
        error.Code.ShouldBe("order.invalid");
        error.Message.ShouldBe("Order is invalid");
        error.Status.ShouldBe(422);
        error.Type.ShouldBe("https://errors.kernel/custom");
        error.Instance.ShouldBe("/orders/42");
        error.Severity.ShouldBe(ErrorSeverity.Critical);
    }

    [Fact]
    public void Error_Should_Implement_IError()
    {
        // Arrange
        // Act
        // Assert
        typeof(IError).IsAssignableFrom(typeof(Error)).ShouldBeTrue();
    }

    #endregion

    #region Guard rails (through the constructor)

    [Fact]
    public void Error_When_Code_Null_Should_Throw_ArgumentNullException()
    {
        // Arrange
        // Act
        var act = () => new Error(null!, "message");

        // Assert
        var ex = Should.Throw<ArgumentNullException>(act);
        ex.ParamName.ShouldBe("code");
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Code.NullOrWhitespace.Code);
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Code.NullOrWhitespace.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Error_When_Code_Whitespace_Should_Throw_ArgumentException(string? code)
    {
        // Arrange
        // Act
        var act = () => new Error(code!, "message");

        // Assert
        var ex = Should.Throw<ArgumentException>(act);
        ex.ParamName.ShouldBe("code");
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Code.NullOrWhitespace.Code);
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Code.NullOrWhitespace.Message);
    }

    [Fact]
    public void Error_When_Code_Exceeds_Max_Length_Should_Throw_Range_With_Formatted_Pattern()
    {
        // Arrange
        var code = new string('c', ErrorConstant.Constraint.Code.MaxLength + 1);

        // Act
        var act = () => new Error(code, "message");

        // Assert
        var ex = Should.Throw<ArgumentOutOfRangeException>(act);
        ex.ParamName.ShouldBe("code");
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Code.ExceedsMaxLength.Code);
        ex.Message.ShouldContain(ErrorConstant.Constraint.Code.MaxLength.ToString());
    }

    [Fact]
    public void Error_When_Message_Null_Should_Throw_ArgumentNullException()
    {
        // Arrange
        // Act
        var act = () => new Error("code", null!);

        // Assert
        var ex = Should.Throw<ArgumentNullException>(act);
        ex.ParamName.ShouldBe("message");
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Message.NullOrWhitespace.Code);
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Message.NullOrWhitespace.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Error_When_Message_Whitespace_Should_Throw_ArgumentException(string? message)
    {
        // Arrange
        // Act
        var act = () => new Error("code", message!);

        // Assert
        var ex = Should.Throw<ArgumentException>(act);
        ex.ParamName.ShouldBe("message");
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Message.NullOrWhitespace.Code);
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Message.NullOrWhitespace.Message);
    }

    [Fact]
    public void Error_When_Message_Exceeds_Max_Length_Should_Throw_Range_With_Formatted_Pattern()
    {
        // Arrange
        var message = new string('m', ErrorConstant.Constraint.Message.MaxLength + 1);

        // Act
        var act = () => new Error("code", message);

        // Assert
        var ex = Should.Throw<ArgumentOutOfRangeException>(act);
        ex.ParamName.ShouldBe("message");
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Message.ExceedsMaxLength.Code);
        ex.Message.ShouldContain(ErrorConstant.Constraint.Message.MaxLength.ToString());
    }

    [Theory]
    [InlineData(99)]
    [InlineData(600)]
    public void Error_When_Status_Out_Of_Range_Should_Throw_With_Formatted_Pattern(int status)
    {
        // Arrange
        // Act
        var act = () => new Error("code", "message", status: status);

        // Assert
        var ex = Should.Throw<ArgumentOutOfRangeException>(act);
        ex.ParamName.ShouldBe("status");
        ex.Message.ShouldContain(ErrorConstant.Result.Failure.Status.OutOfRange.Code);
        ex.Message.ShouldContain(ErrorConstant.Constraint.Status.Min.ToString());
        ex.Message.ShouldContain(ErrorConstant.Constraint.Status.Max.ToString());
    }

    [Theory]
    [InlineData(100)]
    [InlineData(599)]
    public void Error_When_Status_In_Range_Should_Construct(int status)
    {
        // Arrange
        // Act
        var error = new Error("code", "message", status: status);

        // Assert
        error.Status.ShouldBe(status);
    }

    #endregion

    #region Rendering

    [Fact]
    public void Error_ToString_Should_Render_Colon_Joined_Code_And_Message()
    {
        // Arrange
        var error = new Error("order.not_found", "Order was not found");

        // Act
        var actual = error.ToString();

        // Assert
        actual.ShouldBe("order.not_found:Order was not found");
    }

    #endregion

    #region Value equality (metadata excluded)

    [Fact]
    public void Error_Equality_Same_Values_Should_Be_Equal()
    {
        // Arrange
        var left = Error.BadRequest("order.not_found", "Order was not found");
        var right = Error.BadRequest("order.not_found", "Order was not found");

        // Act
        // Assert
        left.Equals((IError?)right).ShouldBeTrue();
    }

    [Fact]
    public void Error_Equality_Should_Ignore_Metadata()
    {
        // Arrange
        var left = new Error("code", "message");
        var right = new Error("code", "message");
        left = left.WithMetadata("trace.id", "abc");

        // Act
        // Assert
        left.Metadata.ShouldNotBeSameAs(right.Metadata);
        left.Equals((IError?)right).ShouldBeTrue();
    }

    [Fact]
    public void Error_Equality_When_Different_Should_Not_Be_Equal()
    {
        // Arrange
        var baseline = new Error("code", "message");

        // Act
        // Assert
        baseline.Equals((IError?)new Error("other", "message")).ShouldBeFalse();
        baseline.Equals((IError?)new Error("code", "other")).ShouldBeFalse();
        baseline.Equals((IError?)new Error("code", "message", status: 500)).ShouldBeFalse();
        baseline.Equals((IError?)new Error("code", "message", severity: ErrorSeverity.Warning)).ShouldBeFalse();
        (baseline == new Error("code", "message", type: "t")).ShouldBeFalse();
        (baseline != new Error("code", "message", instance: "i")).ShouldBeTrue();
    }

    [Fact]
    public void Error_Equality_When_Null_Should_Not_Be_Equal()
    {
        // Arrange
        var error = new Error("code", "message");

        // Act
        // Assert
        error.Equals((IError?)null).ShouldBeFalse();
        error.Equals((Error?)null).ShouldBeFalse();
    }

    [Fact]
    public void Error_GetHashCode_Same_Values_Should_Match()
    {
        // Arrange
        var left = new Error("code", "message");
        var right = new Error("code", "message");

        // Act
        // Assert
        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    #endregion
}