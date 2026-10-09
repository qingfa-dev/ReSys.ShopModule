using BuildingBlock.Core.Domain.Concerns.Foundation.Identifiable;
namespace BuildingBlock.Core.Domain.Tests.Concerns.Foundation.Identifiable;

/// <summary>Specifications for <see cref="IdentifiableExtensions"/> and <see cref="IdentifiableValidator"/>.</summary>
public class IdentifiableExtensionSpec
{
    #region Helper Methods

    private sealed class TestIdentifiable : IIdentifiable<Guid>
    {
        public Guid Id { get; set; }
    }

    private sealed class TestStringIdentifiable : IIdentifiable<string>
    {
        public string Id { get; set; } = string.Empty;
    }

    private sealed class TestIntIdentifiable : IIdentifiable<int>
    {
        public int Id { get; set; }
    }

    /// <summary>Creates a successful result wrapping the given entity.</summary>
    /// <param name="entity">The entity to wrap.</param>
    /// <returns>A successful result containing the entity.</returns>
    private static Result<TestIdentifiable> Success(TestIdentifiable entity)
    {
        return Result<TestIdentifiable>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void EnsureIdentified_WithId_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestIdentifiable { Id = Guid.NewGuid() };

        // Act:
        var result = Success(entity).EnsureIdentified<TestIdentifiable, Guid>();

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(entity);
    }

    [Fact]
    public void EnsureIdentified_DefaultId_ShouldFailIdRequired()
    {
        // Arrange:
        var entity = new TestIdentifiable();

        // Act:
        var result = Success(entity).EnsureIdentified<TestIdentifiable, Guid>();

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Identifiable.Id.Required");
    }

    [Fact]
    public void EnsureIdentified_NullStringId_ShouldFailIdRequired()
    {
        // Arrange:
        var entity = new TestStringIdentifiable { Id = null! };

        // Act:
        var result = Result<TestStringIdentifiable>.Success(entity)
            .EnsureIdentified<TestStringIdentifiable, string>();

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Identifiable.Id.Required");
    }

    [Fact]
    public void ValidateIdentification_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = IdentifiableValidator.ValidateIdentification<
            TestIdentifiable,
            Guid>(null!);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Identifiable.Entity.Required");
    }

    [Fact]
    public void EnsureIdentified_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestIdentifiable>.Fail(
            Error.UnprocessableEntity("Test.Fail", "boom"));

        // Act:
        var result = input.EnsureIdentified<TestIdentifiable, Guid>();

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Test.Fail");
    }

    [Fact]
    public void RequireIdentified_WithId_ShouldReturnEntity()
    {
        // Arrange:
        var entity = new TestIdentifiable { Id = Guid.NewGuid() };

        // Act:
        var value = Success(entity).RequireIdentified<TestIdentifiable, Guid>();

        // Assert:
        value.ShouldBeSameAs(entity);
    }

    [Fact]
    public void RequireIdentified_DefaultId_ShouldThrow()
    {
        // Arrange:
        var entity = new TestIdentifiable();

        // Act:
        // Assert:
        Should.Throw<ResultException>(() => Success(entity).RequireIdentified<TestIdentifiable, Guid>());
    }

    [Fact]
    public void RequireIdentified_BlankStringId_ShouldThrow()
    {
        // Arrange:
        var entity = new TestStringIdentifiable { Id = "   " };

        // Act:
        // Assert:
        Should.Throw<ResultException>(() => Result<TestStringIdentifiable>.Success(entity).RequireIdentified<TestStringIdentifiable, string>());
    }

    [Fact]
    public void RequireIdentified_InputFailure_ShouldThrow()
    {
        // Arrange:
        var input = Result<TestIdentifiable>.Fail(
            Error.UnprocessableEntity("Test.Fail", "boom"));

        // Act:
        // Assert:
        Should.Throw<ResultException>(() => input.RequireIdentified<TestIdentifiable, Guid>());
    }

    #endregion

    #region Validator Specs

    [Fact]
    public void ValidateIdentification_GuidEmpty_ShouldFailIdRequired()
    {
        // Arrange:
        var entity = new TestIdentifiable { Id = Guid.Empty };

        // Act:
        var result = IdentifiableValidator.ValidateIdentification<TestIdentifiable, Guid>(entity);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Identifiable.Id.Required");
    }

    [Fact]
    public void ValidateIdentification_BlankStringId_ShouldFailIdRequired()
    {
        // Arrange:
        var entity = new TestStringIdentifiable { Id = "   " };

        // Act:
        var result = IdentifiableValidator.ValidateIdentification<TestStringIdentifiable, string>(entity);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Identifiable.Id.Required");
    }

    [Fact]
    public void ValidateIdentification_EmptyStringId_ShouldFailIdRequired()
    {
        // Arrange:
        var entity = new TestStringIdentifiable { Id = string.Empty };

        // Act:
        var result = IdentifiableValidator.ValidateIdentification<TestStringIdentifiable, string>(entity);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Identifiable.Id.Required");
    }

    [Fact]
    public void ValidateIdentification_ValidGuid_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestIdentifiable { Id = Guid.NewGuid() };

        // Act:
        var result = IdentifiableValidator.ValidateIdentification<TestIdentifiable, Guid>(entity);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(entity);
    }

    [Fact]
    public void ValidateIdentification_ValidStringId_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestStringIdentifiable { Id = "entity-1" };

        // Act:
        var result = IdentifiableValidator.ValidateIdentification<TestStringIdentifiable, string>(entity);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(entity);
    }

    [Fact]
    public void ValidateId_NullString_ShouldFailIdRequired()
    {
        // Arrange:
        // Act:
        var result = IdentifiableValidator.ValidateId<string>(null!);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Identifiable.Id.Required");
    }

    [Fact]
    public void ValidateId_GuidEmpty_ShouldFailIdRequired()
    {
        // Arrange:
        // Act:
        var result = IdentifiableValidator.ValidateId(Guid.Empty);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Identifiable.Id.Required");
    }

    [Fact]
    public void ValidateId_BlankString_ShouldFailIdRequired()
    {
        // Arrange:
        // Act:
        var result = IdentifiableValidator.ValidateId("   ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Identifiable.Id.Required");
    }

    [Fact]
    public void ValidateId_ValidGuid_ShouldSucceed()
    {
        // Arrange:
        var id = Guid.NewGuid();

        // Act:
        var result = IdentifiableValidator.ValidateId(id);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(id);
    }

    [Fact]
    public void ValidateId_ValidString_ShouldSucceed()
    {
        // Arrange:
        // Act:
        var result = IdentifiableValidator.ValidateId("entity-1");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("entity-1");
    }

    [Fact]
    public void ValidateId_EmptyString_ShouldFailIdRequired()
    {
        // Arrange:
        // Act:
        var result = IdentifiableValidator.ValidateId(string.Empty);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Identifiable.Id.Required");
    }

    [Fact]
    public void ValidateId_DefaultInt_ShouldFailIdRequired()
    {
        // Arrange:
        // Act:
        var result = IdentifiableValidator.ValidateId(0);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Identifiable.Id.Required");
    }

    [Fact]
    public void ValidateId_ValidInt_ShouldSucceed()
    {
        // Arrange:
        // Act:
        var result = IdentifiableValidator.ValidateId(42);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void ValidateIdentification_DefaultInt_ShouldFailIdRequired()
    {
        // Arrange:
        var entity = new TestIntIdentifiable { Id = 0 };

        // Act:
        var result = IdentifiableValidator.ValidateIdentification<TestIntIdentifiable, int>(entity);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Identifiable.Id.Required");
    }

    [Fact]
    public void ValidateIdentification_ValidInt_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestIntIdentifiable { Id = 42 };

        // Act:
        var result = IdentifiableValidator.ValidateIdentification<TestIntIdentifiable, int>(entity);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(entity);
    }

    [Fact]
    public void EnsureIdentified_ValidInt_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestIntIdentifiable { Id = 7 };

        // Act:
        var result = Result<TestIntIdentifiable>.Success(entity).EnsureIdentified<TestIntIdentifiable, int>();

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(entity);
    }

    [Fact]
    public void RequireIdentified_ValidInt_ShouldReturnEntity()
    {
        // Arrange:
        var entity = new TestIntIdentifiable { Id = 7 };

        // Act:
        var value = Result<TestIntIdentifiable>.Success(entity).RequireIdentified<TestIntIdentifiable, int>();

        // Assert:
        value.ShouldBeSameAs(entity);
    }

    [Fact]
    public void Constants_ShouldMatchNestedDefinitions()
    {
        // Arrange:
        // Act:
        // Assert:
        IdentifiableConstant.Constraints.Id.AllowDefault.ShouldBeFalse();
        IdentifiableConstant.Defaults.EmptyId.ShouldBe(Guid.Empty);
    }

    #endregion
}
