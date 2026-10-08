using BuildingBlock.Monad.Errors;

namespace BuildingBlock.Monad.UnitTest.Errors;

/// <summary>Tests for <see cref="Error"/> factory methods.</summary>
/// <remarks>
/// Verifies that <see cref="Error.Custom"/> maps all properties,
/// and that category factories apply the correct status/type/severity defaults.
/// </remarks>
[Trait("Category", "Unit")]
public class ErrorFactorySpec
{
    #region Custom

    [Fact]
    public void Custom_When_All_Values_Provided_Should_Map_Every_Property()
    {
        // Arrange
        // Act
        var error = Error.Custom(
            code: "order.custom",
            message: "Custom failure",
            status: 409,
            type: "https://errors.kernel/custom",
            instance: "/orders/42",
            severity: ErrorSeverity.Warning);

        // Assert
        error.Code.ShouldBe("order.custom");
        error.Message.ShouldBe("Custom failure");
        error.Status.ShouldBe(409);
        error.Type.ShouldBe("https://errors.kernel/custom");
        error.Instance.ShouldBe("/orders/42");
        error.Severity.ShouldBe(ErrorSeverity.Warning);
    }

    [Fact]
    public void Custom_When_Defaults_Should_Apply_Contract_Defaults()
    {
        // Arrange
        // Act
        var error = Error.Custom("code", "message");

        // Assert
        error.Status.ShouldBeNull();
        error.Type.ShouldBeNull();
        error.Instance.ShouldBeNull();
        error.Severity.ShouldBe(ErrorSeverity.Error);
    }

    #endregion

    #region Category factories

     [Theory]
     [InlineData(nameof(Error.BadRequest), 400, "https://errors.kernel/bad-request", ErrorSeverity.Warning)]
     [InlineData(nameof(Error.Unauthorized), 401, "https://errors.kernel/unauthorized", ErrorSeverity.Error)]
     [InlineData(nameof(Error.Forbidden), 403, "https://errors.kernel/forbidden", ErrorSeverity.Error)]
     [InlineData(nameof(Error.NotFound), 404, "https://errors.kernel/not-found", ErrorSeverity.Warning)]
     [InlineData(nameof(Error.Conflict), 409, "https://errors.kernel/conflict", ErrorSeverity.Warning)]
     [InlineData(nameof(Error.UnprocessableEntity), 422, "https://errors.kernel/unprocessable-entity", ErrorSeverity.Warning)]
     [InlineData(nameof(Error.InternalServerError), 500, "https://errors.kernel/internal-server-error", ErrorSeverity.Critical)]
     [InlineData(nameof(Error.ServiceUnavailable), 503, "https://errors.kernel/service-unavailable", ErrorSeverity.Critical)]
     public void Category_Factory_Should_Apply_Category_Shape(
        string factory,
        int status,
        string type,
        ErrorSeverity severity)
    {
        // Arrange
        // Act
        var error = InvokeFactory(factory);

        // Assert
        error.Code.ShouldBe("test.code");
        error.Message.ShouldBe("test message");
        error.Status.ShouldBe(status);
        error.Type.ShouldBe(type);
        error.Severity.ShouldBe(severity);
        error.Instance.ShouldBeNull();
    }

    #endregion

    private static Error InvokeFactory(string factory) => factory switch
    {
        nameof(Error.BadRequest) => Error.BadRequest("test.code", "test message"),
        nameof(Error.Unauthorized) => Error.Unauthorized("test.code", "test message"),
        nameof(Error.Forbidden) => Error.Forbidden("test.code", "test message"),
        nameof(Error.NotFound) => Error.NotFound("test.code", "test message"),
        nameof(Error.Conflict) => Error.Conflict("test.code", "test message"),
        nameof(Error.UnprocessableEntity) => Error.UnprocessableEntity("test.code", "test message"),
        nameof(Error.InternalServerError) => Error.InternalServerError("test.code", "test message"),
        nameof(Error.ServiceUnavailable) => Error.ServiceUnavailable("test.code", "test message"),
        _ => throw new ArgumentOutOfRangeException(nameof(factory), factory, null),
    };
}