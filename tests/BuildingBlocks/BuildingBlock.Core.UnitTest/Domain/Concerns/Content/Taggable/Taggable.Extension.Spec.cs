using BuildingBlock.Core.Domain.Concerns.Content.Taggable;
namespace BuildingBlock.Core.Domain.Tests.Concerns.Content.Taggable;

/// <summary>Spec for <see cref="TaggableExtensions"/> covering extension behavior,
/// <see cref="TaggableValidator"/> branches and <see cref="TaggableConstant"/> values.</summary>
public class TaggableExtensionSpec
{
    #region Helper Methods

    private sealed class TestTaggable : ITaggable
    {
        public ICollection<string> Tags { get; } = new List<string>();
    }

    /// <summary>Creates a successful result wrapping the test entity.</summary>
    /// <param name="entity">The test entity.</param>
    /// <returns>A successful result.</returns>
    private static Result<TestTaggable> Success(
        TestTaggable entity)
    {
        return Result<TestTaggable>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void AddTag_NewTag_ShouldAddTag()
    {
        // Arrange:
        var entity = new TestTaggable();

        // Act:
        var result = Success(entity).AddTag("sale");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.Tags.ShouldHaveSingleItem().ShouldBe("sale");
    }

    [Fact]
    public void AddTag_DuplicateTag_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestTaggable();
        Success(entity).AddTag("sale");

        // Act:
        var result = Success(entity).AddTag("sale");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Taggable.Tag.Duplicate");
        entity.Tags.ShouldHaveSingleItem();
    }

    [Fact]
    public void AddTag_NullTag_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestTaggable();

        // Act:
        var result = Success(entity).AddTag(null);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Taggable.Tag.Required");
        entity.Tags.ShouldBeEmpty();
    }

    [Fact]
    public void AddTag_TooLongTag_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestTaggable();
        var tag = new string(
            't',
            TaggableConstant.Constraints.Tag.MaxLength + 1);

        // Act:
        var result = Success(entity).AddTag(tag);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Taggable.Tag.TooLong");
        entity.Tags.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveTag_ExistingTag_ShouldRemoveTag()
    {
        // Arrange:
        var entity = new TestTaggable();
        Success(entity).AddTag("sale");

        // Act:
        var result = Success(entity).RemoveTag("sale");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.Tags.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveTag_MissingTag_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestTaggable();
        Success(entity).AddTag("sale");

        // Act:
        var result = Success(entity).RemoveTag("clearance");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Taggable.Tag.NotFound");
        entity.Tags.ShouldHaveSingleItem().ShouldBe("sale");
    }

    [Fact]
    public void ValidateAddTag_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = TaggableValidator.ValidateAddTag(
            (TestTaggable)null!,
            "sale");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Taggable.Entity.Required");
    }

    [Fact]
    public void AddTag_PaddedMixedCaseTag_ShouldNormalizeToLowerTrimmed()
    {
        // Arrange:
        var entity = new TestTaggable();

        // Act:
        var result = Success(entity).AddTag(" SALE ");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.Tags.ShouldHaveSingleItem().ShouldBe("sale");
    }

    [Fact]
    public void AddTag_DuplicateAfterNormalization_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestTaggable();
        Success(entity).AddTag("Sale");

        // Act:
        var result = Success(entity).AddTag(" sale ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Taggable.Tag.Duplicate");
        entity.Tags.ShouldHaveSingleItem().ShouldBe("sale");
    }

    [Fact]
    public void ClearTags_WithEntries_ShouldClearTags()
    {
        // Arrange:
        var entity = new TestTaggable();
        Success(entity).AddTag("sale");
        Success(entity).AddTag("new");

        // Act:
        var result = Success(entity).ClearTags();

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.Tags.ShouldBeEmpty();
    }

    [Fact]
    public void ClearTags_EmptyCollection_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestTaggable();

        // Act:
        var result = Success(entity).ClearTags();

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.Tags.ShouldBeEmpty();
    }

    [Fact]
    public void AddTag_WhitespaceTag_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestTaggable();

        // Act:
        var result = Success(entity).AddTag("   ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Taggable.Tag.Required");
        entity.Tags.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveTag_PaddedMixedCase_ShouldNormalizeAndRemove()
    {
        // Arrange:
        var entity = new TestTaggable();
        Success(entity).AddTag("sale");

        // Act:
        var result = Success(entity).RemoveTag(" SALE ");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.Tags.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveTag_NullTag_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestTaggable();
        Success(entity).AddTag("sale");

        // Act:
        var result = Success(entity).RemoveTag(null);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Taggable.Tag.NotFound");
        entity.Tags.ShouldHaveSingleItem().ShouldBe("sale");
    }

    [Fact]
    public void AddTag_InputFailure_ShouldPropagateWithoutMutation()
    {
        // Arrange:
        var entity = new TestTaggable();
        var input = Result<TestTaggable>.Fail(
            Error.UnprocessableEntity("Test.Fail", "boom"));

        // Act:
        var result = input.AddTag("sale");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Test.Fail");
        entity.Tags.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveTag_InputFailure_ShouldPropagateWithoutMutation()
    {
        // Arrange:
        var entity = new TestTaggable();
        entity.Tags.Add("sale");
        var input = Result<TestTaggable>.Fail(
            Error.UnprocessableEntity("Test.Fail", "boom"));

        // Act:
        var result = input.RemoveTag("sale");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Test.Fail");
        entity.Tags.ShouldHaveSingleItem();
    }

    [Fact]
    public void ClearTags_InputFailure_ShouldPropagateWithoutMutation()
    {
        // Arrange:
        var entity = new TestTaggable();
        entity.Tags.Add("sale");
        var input = Result<TestTaggable>.Fail(
            Error.UnprocessableEntity("Test.Fail", "boom"));

        // Act:
        var result = input.ClearTags();

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Test.Fail");
        entity.Tags.ShouldHaveSingleItem();
    }

    [Fact]
    public void NormalizeTag_Null_ShouldReturnNull()
    {
        // Arrange:
        // Act:
        var normalized = TaggableExtensions.NormalizeTag(null);

        // Assert:
        normalized.ShouldBeNull();
    }

    [Fact]
    public void NormalizeTag_PaddedMixedCase_ShouldTrimAndLowercase()
    {
        // Arrange:
        // Act:
        var normalized = TaggableExtensions.NormalizeTag(" SALE ");

        // Assert:
        normalized.ShouldBe("sale");
    }

    [Fact]
    public void NormalizeTag_PlainMixedCase_ShouldLowercase()
    {
        // Arrange:
        // Act:
        var normalized = TaggableExtensions.NormalizeTag("Sale");

        // Assert:
        normalized.ShouldBe("sale");
    }

    #endregion

    #region Validator Specs

    [Fact]
    public void ValidateAddTag_BlankTag_ShouldFailTagRequired()
    {
        // Arrange:
        var entity = new TestTaggable();

        // Act:
        var result = TaggableValidator.ValidateAddTag(entity, "   ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Taggable.Tag.Required");
        entity.Tags.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(" leading")]
    [InlineData("trailing ")]
    public void ValidateAddTag_UntrimmedTag_ShouldFailTagInvalid(string tag)
    {
        // Arrange:
        var entity = new TestTaggable();

        // Act:
        // Note: the extension normalizes (trim + lower) before validating,
        // so untrimmed input only reaches the validator on the direct path.
        var result = TaggableValidator.ValidateAddTag(entity, tag);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Taggable.Tag.Invalid");
        entity.Tags.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateAddTag_TooManyTags_ShouldFailTagsTooMany()
    {
        // Arrange:
        var entity = new TestTaggable();
        for (var i = 0; i < TaggableConstant.Constraints.Collection.MaxTags; i++)
        {
            entity.Tags.Add($"tag{i}");
        }

        // Act:
        var result = TaggableValidator.ValidateAddTag(entity, "overflow");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Taggable.Tags.TooMany");
        entity.Tags.Count.ShouldBe(
            TaggableConstant.Constraints.Collection.MaxTags);
    }

    [Fact]
    public void ValidateRemoveTag_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = TaggableValidator.ValidateRemoveTag(
            (TestTaggable)null!,
            "sale");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Taggable.Entity.Required");
    }

    [Fact]
    public void ValidateRemoveTag_MissingTag_ShouldFailTagNotFound()
    {
        // Arrange:
        var entity = new TestTaggable();
        entity.Tags.Add("sale");

        // Act:
        var result = TaggableValidator.ValidateRemoveTag(entity, "clearance");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Taggable.Tag.NotFound");
        entity.Tags.ShouldHaveSingleItem();
    }

    [Fact]
    public void ValidateRemoveTag_ExistingTag_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestTaggable();
        entity.Tags.Add("sale");

        // Act:
        var result = TaggableValidator.ValidateRemoveTag(entity, "sale");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateClearTags_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = TaggableValidator.ValidateClearTags(
            (TestTaggable)null!);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Taggable.Entity.Required");
    }

    [Fact]
    public void ValidateClearTags_ExistingEntity_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestTaggable();
        entity.Tags.Add("sale");

        // Act:
        var result = TaggableValidator.ValidateClearTags(entity);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        // Arrange:
        // Act:
        // Assert:
        // Note: TagTooShort is shadowed by TagRequired for empty input
        // (MinLength 1), so its code is asserted here rather than via the validator.
        TaggableResult.Failure.EntityRequired.Code.ShouldBe("Taggable.Entity.Required");
        TaggableResult.Failure.TagRequired.Code.ShouldBe("Taggable.Tag.Required");
        TaggableResult.Failure.TagTooShort.Code.ShouldBe("Taggable.Tag.TooShort");
        TaggableResult.Failure.TagTooLong.Code.ShouldBe("Taggable.Tag.TooLong");
        TaggableResult.Failure.TagInvalid.Code.ShouldBe("Taggable.Tag.Invalid");
        TaggableResult.Failure.TagDuplicate.Code.ShouldBe("Taggable.Tag.Duplicate");
        TaggableResult.Failure.TagNotFound.Code.ShouldBe("Taggable.Tag.NotFound");
        TaggableResult.Failure.TagsTooMany.Code.ShouldBe("Taggable.Tags.TooMany");
    }

    #endregion

    #region Constant Specs

    [Fact]
    public void Constants_ShouldMatchDocumentedValues()
    {
        // Arrange:
        // Act:
        // Assert:
        TaggableConstant.Constraints.Tag.MinLength.ShouldBe(1);
        TaggableConstant.Constraints.Tag.MaxLength.ShouldBe(128);
        TaggableConstant.Constraints.Collection.MinTags.ShouldBe(0);
        TaggableConstant.Constraints.Collection.MaxTags.ShouldBe(50);
        TaggableConstant.Defaults.Normalization.TrimWhitespace.ShouldBeTrue();
        TaggableConstant.Defaults.Normalization.ToLowercase.ShouldBeTrue();
        TaggableConstant.Patterns.Tag.ShouldBe(@"^\S(?:.*\S)?$");
    }

    #endregion
}
