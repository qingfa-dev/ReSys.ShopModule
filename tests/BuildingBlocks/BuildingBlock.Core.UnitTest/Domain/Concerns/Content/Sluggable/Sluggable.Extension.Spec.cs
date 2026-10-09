using BuildingBlock.Core.Domain.Concerns.Content.Sluggable;
namespace BuildingBlock.Core.Domain.Tests.Concerns.Content.Sluggable;

/// <summary>Spec for <see cref="SluggableExtensions"/> covering extension behavior,
/// <see cref="SluggableValidator"/> branches and <see cref="SluggableConstant"/> values.</summary>
public class SluggableExtensionSpec
{
    #region Helper Methods

    private sealed class TestSluggable : ISluggable
    {
        public string Slug { get; set; } = string.Empty;
    }

    /// <summary>Creates a successful result wrapping the test entity.</summary>
    /// <param name="entity">The test entity.</param>
    /// <returns>A successful result.</returns>
    private static Result<TestSluggable> Success(
        TestSluggable entity)
    {
        return Result<TestSluggable>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void SetSlug_ValidSlug_ShouldSetSlug()
    {
        // Arrange:
        var entity = new TestSluggable();

        // Act:
        var result = Success(entity).SetSlug("hello-world");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.Slug.ShouldBe("hello-world");
    }

    [Fact]
    public void SetSlug_NullSlug_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };

        // Act:
        var result = Success(entity).SetSlug(null);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Sluggable.Slug.Required");
        entity.Slug.ShouldBe("keep-me");
    }

    [Fact]
    public void SetSlug_UppercaseSlug_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };

        // Act:
        var result = Success(entity).SetSlug("Hello-World");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Sluggable.Slug.Invalid");
        entity.Slug.ShouldBe("keep-me");
    }

    [Fact]
    public void SetSlug_TooLongSlug_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };
        var slug = new string(
            's',
            SluggableConstant.Constraints.Slug.MaxLength + 1);

        // Act:
        var result = Success(entity).SetSlug(slug);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Sluggable.Slug.TooLong");
        entity.Slug.ShouldBe("keep-me");
    }

    [Fact]
    public void SetSlugFromText_PlainText_ShouldSlugify()
    {
        // Arrange:
        var entity = new TestSluggable();

        // Act:
        var result = Success(entity)
            .SetSlugFromText("Hello, World!");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.Slug.ShouldBe("hello-world");
    }

    [Fact]
    public void SetSlugFromText_Diacritics_ShouldFoldToAscii()
    {
        // Arrange:
        var entity = new TestSluggable();

        // Act:
        var result = Success(entity)
            .SetSlugFromText("Crème Brûlée");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.Slug.ShouldBe("creme-brulee");
    }

    [Fact]
    public void SetSlugFromText_SymbolsOnly_ShouldFailSlugRequired()
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };

        // Act:
        var result = Success(entity).SetSlugFromText("!!!");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Sluggable.Slug.Required");
        entity.Slug.ShouldBe("keep-me");
    }

    [Fact]
    public void ValidateSlug_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = SluggableValidator.ValidateSlug(
            (TestSluggable)null!,
            "hello-world");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Sluggable.Entity.Required");
    }

    [Fact]
    public void SetSlug_WhitespaceSlug_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };

        // Act:
        var result = Success(entity).SetSlug("   ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Sluggable.Slug.Required");
        entity.Slug.ShouldBe("keep-me");
    }

    [Fact]
    public void SetSlugFromText_NullText_ShouldFailSlugRequired()
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };

        // Act:
        var result = Success(entity).SetSlugFromText(null);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Sluggable.Slug.Required");
        entity.Slug.ShouldBe("keep-me");
    }

    [Fact]
    public void SetSlugFromText_WhitespaceText_ShouldFailSlugRequired()
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };

        // Act:
        var result = Success(entity).SetSlugFromText("   ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Sluggable.Slug.Required");
        entity.Slug.ShouldBe("keep-me");
    }

    [Fact]
    public void SetSlugFromText_TrailingSeparator_ShouldTrimHyphen()
    {
        // Arrange:
        var entity = new TestSluggable();

        // Act:
        var result = Success(entity).SetSlugFromText("Hello World! ");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.Slug.ShouldBe("hello-world");
    }

    #endregion

    #region Validator Specs

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateSlug_BlankSlug_ShouldFailSlugRequired(string? slug)
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };

        // Act:
        var result = SluggableValidator.ValidateSlug(entity, slug);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Sluggable.Slug.Required");
        entity.Slug.ShouldBe("keep-me");
    }

    [Fact]
    public void ValidateSlug_TooLongSlug_ShouldFailSlugTooLong()
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };
        var slug = new string(
            's',
            SluggableConstant.Constraints.Slug.MaxLength + 1);

        // Act:
        var result = SluggableValidator.ValidateSlug(entity, slug);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Sluggable.Slug.TooLong");
        entity.Slug.ShouldBe("keep-me");
    }

    [Theory]
    [InlineData("Hello-World")]
    [InlineData("hello--world")]
    [InlineData("hello_world")]
    [InlineData("-leading")]
    [InlineData("trailing-")]
    public void ValidateSlug_InvalidSlug_ShouldFailSlugInvalid(string slug)
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };

        // Act:
        var result = SluggableValidator.ValidateSlug(entity, slug);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Sluggable.Slug.Invalid");
        entity.Slug.ShouldBe("keep-me");
    }

    [Fact]
    public void ValidateSlug_ValidSlug_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestSluggable();

        // Act:
        var result = SluggableValidator.ValidateSlug(entity, "hello-world-123");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void Slugify_BlankText_ShouldReturnEmpty(string? text, string expected)
    {
        // Arrange:
        // Act:
        var slug = SluggableExtensions.Slugify(text);

        // Assert:
        slug.ShouldBe(expected);
    }

    [Fact]
    public void Slugify_PaddedText_ShouldTrimHyphens()
    {
        // Arrange:
        // Act:
        var slug = SluggableExtensions.Slugify("--Hello--");

        // Assert:
        slug.ShouldBe("hello");
    }

    [Fact]
    public void Slugify_SymbolsOnly_ShouldReturnEmpty()
    {
        // Arrange:
        // Act:
        var slug = SluggableExtensions.Slugify("!!!");

        // Assert:
        slug.ShouldBe(string.Empty);
    }

    [Fact]
    public void Slugify_ConsecutiveSeparators_ShouldCollapseToSingleHyphen()
    {
        // Arrange:
        // Act:
        var slug = SluggableExtensions.Slugify("hello   world");

        // Assert:
        slug.ShouldBe("hello-world");
    }

    [Fact]
    public void Slugify_TrailingSeparator_ShouldTrimHyphen()
    {
        // Arrange:
        // Act:
        var slug = SluggableExtensions.Slugify("hello-");

        // Assert:
        slug.ShouldBe("hello");
    }

    [Fact]
    public void Slugify_LeadingSeparator_ShouldTrimHyphen()
    {
        // Arrange:
        // Act:
        var slug = SluggableExtensions.Slugify("   hello");

        // Assert:
        slug.ShouldBe("hello");
    }

    [Fact]
    public void Slugify_Digits_ShouldPreserveDigits()
    {
        // Arrange:
        // Act:
        var slug = SluggableExtensions.Slugify("product 123");

        // Assert:
        slug.ShouldBe("product-123");
    }

    [Fact]
    public void Slugify_MixedSeparators_ShouldCollapseToSingleHyphens()
    {
        // Arrange:
        // Act:
        var slug = SluggableExtensions.Slugify("a--b__c");

        // Assert:
        slug.ShouldBe("a-b-c");
    }

    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        // Arrange:
        // Act:
        // Assert:
        // Note: SlugTooShort is shadowed by SlugRequired for empty input
        // (MinLength 1), so its code is asserted here rather than via the validator.
        SluggableResult.Failure.EntityRequired.Code.ShouldBe("Sluggable.Entity.Required");
        SluggableResult.Failure.SlugRequired.Code.ShouldBe("Sluggable.Slug.Required");
        SluggableResult.Failure.SlugTooShort.Code.ShouldBe("Sluggable.Slug.TooShort");
        SluggableResult.Failure.SlugTooLong.Code.ShouldBe("Sluggable.Slug.TooLong");
        SluggableResult.Failure.SlugInvalid.Code.ShouldBe("Sluggable.Slug.Invalid");
    }

    #endregion

    #region Constant Specs

    [Fact]
    public void Constants_ShouldMatchDocumentedValues()
    {
        // Arrange:
        // Act:
        // Assert:
        SluggableConstant.Constraints.Slug.MinLength.ShouldBe(1);
        SluggableConstant.Constraints.Slug.MaxLength.ShouldBe(200);
        SluggableConstant.Defaults.FallbackSlug.ShouldBe("n-a");
        SluggableConstant.Patterns.Slug.ShouldBe(@"^[a-z0-9]+(?:-[a-z0-9]+)*$");
    }

    #endregion
}
