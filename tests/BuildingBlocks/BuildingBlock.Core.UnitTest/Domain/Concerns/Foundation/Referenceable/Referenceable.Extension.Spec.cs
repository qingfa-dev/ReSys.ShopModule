using BuildingBlock.Core.Domain.Concerns.Foundation.Referenceable;
namespace BuildingBlock.Core.Domain.Tests.Concerns.Foundation.Referenceable;

/// <summary>Specifications for <see cref="ReferenceableExtensions"/> and <see cref="ReferenceableValidator"/>.</summary>
public class ReferenceableExtensionSpec
{
    #region Helper Methods

    private sealed class TestReferenceable : IReferenceable
    {
        public string Reference { get; set; } = string.Empty;
    }

    /// <summary>Creates a successful result wrapping the given entity.</summary>
    /// <param name="entity">The entity to wrap.</param>
    /// <returns>A successful result containing the entity.</returns>
    private static Result<TestReferenceable> Success(
        TestReferenceable entity)
    {
        return Result<TestReferenceable>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void SetReference_ValidReference_ShouldSetReference()
    {
        // Arrange:
        var entity = new TestReferenceable();

        // Act:
        var result = Success(entity).SetReference("ORD-2024-0001");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.Reference.ShouldBe("ORD-2024-0001");
    }

    [Fact]
    public void SetReference_WhitespaceReference_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "keep-me" };

        // Act:
        var result = Success(entity).SetReference("   ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Referenceable.Reference.Required");
        entity.Reference.ShouldBe("keep-me");
    }

    [Fact]
    public void SetReference_TooLongReference_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "keep-me" };
        var reference = new string(
            'r',
            ReferenceableConstant.Constraints.MaxReferenceLength + 1);

        // Act:
        var result = Success(entity).SetReference(reference);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Referenceable.Reference.TooLong");
        entity.Reference.ShouldBe("keep-me");
    }

    [Fact]
    public void ValidateReference_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = ReferenceableValidator.ValidateReference(
            (TestReferenceable)null!,
            "ORD-1");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Referenceable.Entity.Required");
    }

    [Fact]
    public void SetReference_NullReference_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "keep-me" };

        // Act:
        var result = Success(entity).SetReference(null);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Referenceable.Reference.Required");
        entity.Reference.ShouldBe("keep-me");
    }

    [Fact]
    public void SetReference_UntrimmedReference_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "keep-me" };

        // Act:
        var result = Success(entity).SetReference(" ORD-1");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Referenceable.Reference.Untrimmed");
        entity.Reference.ShouldBe("keep-me");
    }

    [Fact]
    public void SetReference_InvalidCharsetReference_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "keep-me" };

        // Act:
        var result = Success(entity).SetReference("ORD@001!");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Referenceable.Reference.InvalidCharset");
        entity.Reference.ShouldBe("keep-me");
    }

    [Fact]
    public void SetReference_AtMaxLength_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestReferenceable();
        var reference = new string('r', ReferenceableConstant.Constraints.Reference.MaxLength);

        // Act:
        var result = Success(entity).SetReference(reference);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.Reference.ShouldBe(reference);
    }

    [Fact]
    public void SetReference_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestReferenceable>.Fail(
            Error.UnprocessableEntity("Test.Fail", "boom"));

        // Act:
        var result = input.SetReference("ORD-1");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Test.Fail");
    }

    [Fact]
    public void ClearReference_NonEmpty_ShouldClearToEmpty()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "ORD-2024-0001" };

        // Act:
        var result = Success(entity).ClearReference();

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(entity);
        entity.Reference.ShouldBe(ReferenceableConstant.Defaults.Empty);
    }

    [Fact]
    public void ClearReference_AlreadyEmpty_ShouldStayEmpty()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = ReferenceableConstant.Defaults.Empty };

        // Act:
        var result = Success(entity).ClearReference();

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.Reference.ShouldBe(string.Empty);
    }

    [Fact]
    public void ClearReference_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestReferenceable>.Fail(
            Error.UnprocessableEntity("Test.Fail", "boom"));

        // Act:
        var result = input.ClearReference();

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Test.Fail");
    }

    [Fact]
    public void EnsureReferenced_ValidReference_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "ORD-2024-0001" };

        // Act:
        var result = Success(entity).EnsureReferenced();

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(entity);
    }

    [Fact]
    public void EnsureReferenced_EmptyReference_ShouldFailReferenceRequired()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = string.Empty };

        // Act:
        var result = Success(entity).EnsureReferenced();

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Referenceable.Reference.Required");
    }

    [Fact]
    public void EnsureReferenced_TooLongReference_ShouldFailTooLong()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = new string('r', ReferenceableConstant.Constraints.Reference.MaxLength + 1) };

        // Act:
        var result = Success(entity).EnsureReferenced();

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Referenceable.Reference.TooLong");
    }

    [Fact]
    public void EnsureReferenced_UntrimmedReference_ShouldFailUntrimmed()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "ORD-1 " };

        // Act:
        var result = Success(entity).EnsureReferenced();

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Referenceable.Reference.Untrimmed");
    }

    [Fact]
    public void EnsureReferenced_InvalidCharsetReference_ShouldFailInvalidCharset()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "ORD@001!" };

        // Act:
        var result = Success(entity).EnsureReferenced();

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Referenceable.Reference.InvalidCharset");
    }

    [Fact]
    public void EnsureReferenced_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestReferenceable>.Fail(
            Error.UnprocessableEntity("Test.Fail", "boom"));

        // Act:
        var result = input.EnsureReferenced();

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Test.Fail");
    }

    [Fact]
    public void RequireReferenced_ValidReference_ShouldReturnEntity()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = "ORD-2024-0001" };

        // Act:
        var value = Success(entity).RequireReferenced();

        // Assert:
        value.ShouldBeSameAs(entity);
    }

    [Fact]
    public void RequireReferenced_EmptyReference_ShouldThrow()
    {
        // Arrange:
        var entity = new TestReferenceable { Reference = string.Empty };

        // Act:
        // Assert:
        Should.Throw<ResultException>(() => Success(entity).RequireReferenced());
    }

    [Fact]
    public void RequireReferenced_InputFailure_ShouldThrow()
    {
        // Arrange:
        var input = Result<TestReferenceable>.Fail(
            Error.UnprocessableEntity("Test.Fail", "boom"));

        // Act:
        // Assert:
        Should.Throw<ResultException>(() => input.RequireReferenced());
    }

    #endregion

    #region Validator Specs

    [Fact]
    public void ValidateReference_NullReference_ShouldFailReferenceRequired()
    {
        // Arrange:
        var entity = new TestReferenceable();

        // Act:
        var result = ReferenceableValidator.ValidateReference(entity, null);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Referenceable.Reference.Required");
    }

    [Fact]
    public void ValidateReference_WhitespaceReference_ShouldFailReferenceRequired()
    {
        // Arrange:
        var entity = new TestReferenceable();

        // Act:
        var result = ReferenceableValidator.ValidateReference(entity, "   ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Referenceable.Reference.Required");
    }

    [Fact]
    public void ValidateReference_TooLongReference_ShouldFailTooLong()
    {
        // Arrange:
        var entity = new TestReferenceable();
        var reference = new string('r', ReferenceableConstant.Constraints.Reference.MaxLength + 1);

        // Act:
        var result = ReferenceableValidator.ValidateReference(entity, reference);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Referenceable.Reference.TooLong");
    }

    [Fact]
    public void ValidateReference_LeadingSpaceReference_ShouldFailUntrimmed()
    {
        // Arrange:
        var entity = new TestReferenceable();

        // Act:
        var result = ReferenceableValidator.ValidateReference(entity, " ORD-1");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Referenceable.Reference.Untrimmed");
    }

    [Fact]
    public void ValidateReference_TrailingSpaceReference_ShouldFailUntrimmed()
    {
        // Arrange:
        var entity = new TestReferenceable();

        // Act:
        var result = ReferenceableValidator.ValidateReference(entity, "ORD-1 ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Referenceable.Reference.Untrimmed");
    }

    [Fact]
    public void ValidateReference_BadCharsetReference_ShouldFailInvalidCharset()
    {
        // Arrange:
        var entity = new TestReferenceable();

        // Act:
        var result = ReferenceableValidator.ValidateReference(entity, "ORD@001!");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Referenceable.Reference.InvalidCharset");
    }

    [Fact]
    public void ValidateReference_ValidReference_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestReferenceable();

        // Act:
        var result = ReferenceableValidator.ValidateReference(entity, "ORD-2024-0001");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(entity);
    }

    [Fact]
    public void Constants_ShouldMatchNestedDefinitions()
    {
        // Arrange:
        // Act:
        // Assert:
        ReferenceableConstant.Constraints.Reference.MinLength.ShouldBe(1);
        ReferenceableConstant.Constraints.Reference.MaxLength.ShouldBe(128);
        ReferenceableConstant.Constraints.MaxReferenceLength.ShouldBe(ReferenceableConstant.Constraints.Reference.MaxLength);
        ReferenceableConstant.Defaults.Empty.ShouldBe(string.Empty);
        ReferenceableConstant.Patterns.Trimmed.ShouldBe(@"^\S(?:.*\S)?$");
        ReferenceableConstant.Patterns.AllowedCharset.ShouldBe(@"^[A-Za-z0-9\-_/#:. ]+$");
    }

    #endregion
}
