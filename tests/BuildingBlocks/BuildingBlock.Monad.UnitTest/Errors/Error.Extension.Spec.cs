using BuildingBlock.Monad.Errors;
using BuildingBlock.Monad.Metadata;

namespace BuildingBlock.Monad.UnitTest.Errors;

/// <summary>Tests for <see cref="Error"/> extension methods.</summary>
/// <remarks>
/// Covers <see cref="Error.CopyWith"/> override semantics,
/// <see cref="Error.WithMetadata"/> mutation, well-known metadata helpers,
/// <see cref="Error.WithCode"/>/<see cref="Error.WithMessage"/> copy helpers,
/// and <see cref="Error.FromException"/> mapping.
/// </remarks>
[Trait("Category", "Unit")]
public class ErrorExtensionSpec
{
    #region CopyWith

    [Fact]
    public void CopyWith_When_No_Overrides_Should_Preserve_All_Values()
    {
        // Arrange
        var original = new Error(
            code: "code",
            message: "message",
            status: 409,
            type: "https://errors.kernel/conflict",
            instance: "/orders/42",
            severity: ErrorSeverity.Warning);

        // Act
        var copy = original.CopyWith();

        // Assert
        copy.ShouldNotBeSameAs(original);
        copy.Equals(original).ShouldBeTrue();
        copy.Code.ShouldBe("code");
        copy.Message.ShouldBe("message");
        copy.Status.ShouldBe(409);
        copy.Type.ShouldBe("https://errors.kernel/conflict");
        copy.Instance.ShouldBe("/orders/42");
        copy.Severity.ShouldBe(ErrorSeverity.Warning);
    }

    [Fact]
    public void CopyWith_When_Overrides_Provided_Should_Replace_Only_Those()
    {
        // Arrange
        var original = new Error("code", "message", status: 409, severity: ErrorSeverity.Warning);

        // Act
        var copy = original.CopyWith(message: "changed", severity: ErrorSeverity.Critical);

        // Assert
        copy.Message.ShouldBe("changed");
        copy.Severity.ShouldBe(ErrorSeverity.Critical);
        copy.Code.ShouldBe("code");
        copy.Status.ShouldBe(409);
        original.Message.ShouldBe("message");
        original.Severity.ShouldBe(ErrorSeverity.Warning);
    }

    [Fact]
    public void CopyWith_When_Override_Null_Should_Keep_Original_Value()
    {
        // Arrange
        var original = new Error("code", "message", status: 409);

        // Act
        var copy = original.CopyWith(code: null, message: null, status: null);

        // Assert
        copy.Code.ShouldBe("code");
        copy.Message.ShouldBe("message");
        copy.Status.ShouldBe(409);
    }

    [Fact]
    public void CopyWith_When_Metadata_Provided_Should_Assign_Provided_Bag()
    {
        // Arrange
        var original = new Error("code", "message");
        var metadata = MetadataDictionary.Create().With("trace.id", "abc");

        // Act
        var copy = original.CopyWith(metadata: metadata);

        // Assert
        copy.Metadata.ShouldBeSameAs(metadata);
        original.Metadata.ShouldNotBeSameAs(metadata);
    }

    [Fact]
    public void CopyWith_When_Metadata_Not_Provided_Should_Share_Original_Bag()
    {
        // Arrange
        var original = new Error("code", "message");

        // Act
        var copy = original.CopyWith();

        // Assert
        copy.Metadata.ShouldBeSameAs(original.Metadata);
    }

    #endregion

    #region WithMetadata

    [Fact]
    public void WithMetadata_Key_Value_Should_Mutate_And_Return_Same_Instance()
    {
        // Arrange
        var error = new Error("code", "message");

        // Act
        var result = error.WithMetadata("trace.id", "abc");

        // Assert
        result.ShouldBeSameAs(error);
        error.Metadata["trace.id"].ShouldBe("abc");
    }

    [Fact]
    public void WithMetadata_Dictionary_Should_Replace_Bag()
    {
        // Arrange
        var error = new Error("code", "message");
        var metadata = MetadataDictionary.Create().With("attempt", 3);

        // Act
        var result = error.WithMetadata(metadata);

        // Assert
        result.ShouldBeSameAs(error);
        error.Metadata.ShouldBeSameAs(metadata);
        error.Metadata["attempt"].ShouldBe(3);
    }

    [Fact]
    public void WithMetadata_When_Null_Dictionary_Should_Fall_Back_To_Empty_Bag()
    {
        // Arrange
        var error = new Error("code", "message");

        // Act
        var result = error.WithMetadata((MetadataDictionary)null!);

        // Assert
        result.ShouldBeSameAs(error);
        error.Metadata.ShouldNotBeNull();
        error.Metadata.ShouldBeEmpty();
    }

    [Fact]
    public void With_Metadata_Helpers_Should_Set_Contract_Keys()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.Zero);

        // Act
        var error = new Error("code", "message")
            .WithTraceId("trace-1")
            .WithTimestamp(timestamp)
            .WithResource("orders")
            .WithField("total")
            .WithAttempt(3);

        // Assert
        error.Metadata[ErrorConstant.Metadata.TraceId].ShouldBe("trace-1");
        error.Metadata[ErrorConstant.Metadata.Timestamp].ShouldBe(timestamp);
        error.Metadata[ErrorConstant.Metadata.Resource].ShouldBe("orders");
        error.Metadata[ErrorConstant.Metadata.Field].ShouldBe("total");
        error.Metadata[ErrorConstant.Metadata.Attempt].ShouldBe(3);
    }

    #endregion

    #region With* copy helpers

    [Fact]
    public void With_Extensions_Should_Override_Individual_Fields_On_Copies()
    {
        // Arrange
        var original = new Error(
            code: "code",
            message: "message",
            status: 400,
            type: "https://errors.kernel/bad-request",
            instance: "/orders/42",
            severity: ErrorSeverity.Warning);

        // Act
        // Assert
        original.WithCode("code2").Code.ShouldBe("code2");
        original.WithMessage("message2").Message.ShouldBe("message2");
        original.WithType("type2").Type.ShouldBe("type2");
        original.WithInstance("instance2").Instance.ShouldBe("instance2");
        original.WithStatus(422).Status.ShouldBe(422);
        original.WithSeverity(ErrorSeverity.Critical).Severity.ShouldBe(ErrorSeverity.Critical);
        original.Code.ShouldBe("code");
        original.Severity.ShouldBe(ErrorSeverity.Warning);
    }

    #endregion

    #region FromException

    [Fact]
    public void FromException_When_Null_Should_Throw_Exception_Param()
    {
        // Arrange
        // Act
        var act = () => Error.FromException(null!);

        // Assert
        var ex = Should.Throw<ArgumentNullException>(act);
        ex.ParamName.ShouldBe("exception");
    }

    [Fact]
    public void FromException_When_Provided_Should_Map_To_Internal_Server_Error_With_Metadata()
    {
        // Arrange
        var exception = new InvalidOperationException("boom");

        // Act
        var error = Error.FromException(exception);

        // Assert
        error.Code.ShouldBe("general.error");
        error.Message.ShouldBe("boom");
        error.Status.ShouldBe(500);
        error.Type.ShouldBe("https://errors.kernel/internal-server-error");
        error.Severity.ShouldBe(ErrorSeverity.Critical);
        error.Metadata["exception.type"].ShouldBe(typeof(InvalidOperationException).FullName);
    }

    [Fact]
    public void FromException_When_Custom_Code_And_Severity_Should_Override_Defaults()
    {
        // Arrange
        var exception = new InvalidOperationException("boom");

        // Act
        var error = Error.FromException(exception, code: "db.failure", severity: ErrorSeverity.Warning);

        // Assert
        error.Code.ShouldBe("db.failure");
        error.Severity.ShouldBe(ErrorSeverity.Warning);
        error.Status.ShouldBe(500);
        error.Message.ShouldBe("boom");
    }

    #endregion
}