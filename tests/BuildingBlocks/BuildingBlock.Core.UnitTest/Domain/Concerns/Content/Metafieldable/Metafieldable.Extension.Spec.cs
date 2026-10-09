using BuildingBlock.Core.Domain.Concerns.Content.Metafieldable;
namespace BuildingBlock.Core.Domain.Tests.Concerns.Content.Metafieldable;

/// <summary>Spec for <see cref="MetafieldableExtensions"/> covering extension behavior,
/// <see cref="MetafieldableValidator"/> branches and <see cref="MetafieldableConstant"/> values.</summary>
public class MetafieldableExtensionSpec
{
    #region Helper Methods

    private sealed class TestMetafieldable : IMetafieldable
    {
        public ICollection<Metafield> Metafields { get; } =
            new List<Metafield>();
    }

    /// <summary>Creates a successful result wrapping the test entity.</summary>
    /// <param name="entity">The test entity.</param>
    /// <returns>A successful result.</returns>
    private static Result<TestMetafieldable> Success(
        TestMetafieldable entity)
    {
        return Result<TestMetafieldable>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void SetMetafield_NewKey_ShouldAddMetafield()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        var metafield = entity.Metafields.ShouldHaveSingleItem();
        metafield.Namespace.ShouldBe("specs");
        metafield.Key.ShouldBe("weight");
        metafield.Type.ShouldBe(MetafieldType.Number);
        metafield.Value.ShouldBe("1.5");
    }

    [Fact]
    public void SetMetafield_ExistingKey_ShouldUpdateTypeAndValue()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.ShortText, "old");

        // Act:
        var result = Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "2.0");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        var metafield = entity.Metafields.ShouldHaveSingleItem();
        metafield.Type.ShouldBe(MetafieldType.Number);
        metafield.Value.ShouldBe("2.0");
    }

    [Fact]
    public void SetMetafield_InvalidNamespace_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = Success(entity)
            .SetMetafield("Bad NS", "weight", MetafieldType.Number, "1.5");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe(
            "Metafieldable.Namespace.Invalid");
        entity.Metafields.ShouldBeEmpty();
    }

    [Fact]
    public void SetMetafield_TooLongNamespace_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        var @namespace = new string(
            'n',
            MetafieldableConstant.Constraints.Namespace.MaxLength + 1);

        // Act:
        var result = Success(entity)
            .SetMetafield(@namespace, "weight", MetafieldType.Number, "1.5");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe(
            "Metafieldable.Namespace.TooLong");
        entity.Metafields.ShouldBeEmpty();
    }

    [Fact]
    public void SetMetafield_NullKey_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = Success(entity)
            .SetMetafield("specs", null, MetafieldType.Number, "1.5");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Metafieldable.Key.Required");
        entity.Metafields.ShouldBeEmpty();
    }

    [Fact]
    public void SetMetafield_InvalidKey_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = Success(entity)
            .SetMetafield("specs", "Bad-Key", MetafieldType.Number, "1.5");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Metafieldable.Key.Invalid");
        entity.Metafields.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveMetafield_ExistingKey_ShouldRemoveMetafield()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Act:
        var result = Success(entity).RemoveMetafield("specs", "weight");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.Metafields.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveMetafield_MissingKey_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Act:
        var result = Success(entity).RemoveMetafield("specs", "depth");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe(
            "Metafieldable.Metafield.NotFound");
        entity.Metafields.ShouldHaveSingleItem();
    }

    [Fact]
    public void ValidateMetafield_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = MetafieldableValidator.ValidateMetafield(
            (TestMetafieldable)null!,
            "specs",
            "weight");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Metafieldable.Entity.Required");
    }

    [Fact]
    public void SetMetafield_DefaultType_ShouldUseShortText()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = Success(entity)
            .SetMetafield("specs", "weight", "1.5");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        var metafield = entity.Metafields.ShouldHaveSingleItem();
        metafield.Type.ShouldBe(MetafieldType.ShortText);
        metafield.Value.ShouldBe("1.5");
    }

    [Fact]
    public void ClearMetafields_WithEntries_ShouldClearMetafields()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Act:
        var result = Success(entity).ClearMetafields();

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.Metafields.ShouldBeEmpty();
    }

    [Fact]
    public void ClearMetafields_EmptyCollection_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = Success(entity).ClearMetafields();

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.Metafields.ShouldBeEmpty();
    }

    [Fact]
    public void SetMetafield_NullValue_ShouldSucceedWithNullStored()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, null);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        var metafield = entity.Metafields.ShouldHaveSingleItem();
        metafield.Value.ShouldBeNull();
    }

    [Fact]
    public void SetMetafield_DefaultTypeNullValue_ShouldSucceedWithNullStored()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = Success(entity).SetMetafield("specs", "weight", null);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        var metafield = entity.Metafields.ShouldHaveSingleItem();
        metafield.Type.ShouldBe(MetafieldType.ShortText);
        metafield.Value.ShouldBeNull();
    }

    [Fact]
    public void SetMetafield_UpdateExistingWhenFull_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        for (var i = 0; i < MetafieldableConstant.Constraints.Collection.MaxMetafields; i++)
        {
            entity.Metafields.Add(new Metafield
            {
                Namespace = "ns",
                Key = $"key{i}",
                Type = MetafieldType.ShortText,
                Value = "v"
            });
        }

        // Act:
        var result = Success(entity)
            .SetMetafield("ns", "key0", MetafieldType.Number, "updated");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.Metafields.Count.ShouldBe(
            MetafieldableConstant.Constraints.Collection.MaxMetafields);
        entity.Metafields.Single(m => m.Key == "key0").Value.ShouldBe("updated");
    }

    [Fact]
    public void SetMetafield_InputFailure_ShouldPropagateWithoutMutation()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        var input = Result<TestMetafieldable>.Fail(
            Error.UnprocessableEntity("Test.Fail", "boom"));

        // Act:
        var result = input.SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Test.Fail");
        entity.Metafields.ShouldBeEmpty();
    }

    [Fact]
    public void GetMetafield_ExistingKey_ShouldReturnMetafield()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Act:
        var metafield = entity.GetMetafield("specs", "weight");

        // Assert:
        metafield.ShouldNotBeNull();
        metafield.Namespace.ShouldBe("specs");
        metafield.Key.ShouldBe("weight");
        metafield.Value.ShouldBe("1.5");
    }

    [Fact]
    public void GetMetafield_MissingKey_ShouldReturnNull()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Act:
        var metafield = entity.GetMetafield("specs", "depth");

        // Assert:
        metafield.ShouldBeNull();
    }

    [Fact]
    public void GetMetafield_DifferentNamespace_ShouldReturnNull()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Act:
        var metafield = entity.GetMetafield("other", "weight");

        // Assert:
        metafield.ShouldBeNull();
    }

    [Fact]
    public void GetMetafield_NullEntity_ShouldReturnNull()
    {
        // Arrange:
        TestMetafieldable entity = null!;

        // Act:
        var metafield = entity.GetMetafield("specs", "weight");

        // Assert:
        metafield.ShouldBeNull();
    }

    [Fact]
    public void GetMetafield_NullNamespace_ShouldReturnNull()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Act:
        var metafield = entity.GetMetafield(null, "weight");

        // Assert:
        metafield.ShouldBeNull();
    }

    [Fact]
    public void GetMetafield_NullKey_ShouldReturnNull()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Act:
        var metafield = entity.GetMetafield("specs", null);

        // Assert:
        metafield.ShouldBeNull();
    }

    #endregion

    #region Validator Specs

    [Fact]
    public void ValidateMetafield_BlankNamespace_ShouldFailNamespaceRequired()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = MetafieldableValidator.ValidateMetafield(
            entity,
            "   ",
            "weight",
            "1.5");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Metafieldable.Namespace.Required");
        entity.Metafields.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateMetafield_TooLongKey_ShouldFailKeyTooLong()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        var key = new string(
            'k',
            MetafieldableConstant.Constraints.Key.MaxLength + 1);

        // Act:
        var result = MetafieldableValidator.ValidateMetafield(
            entity,
            "specs",
            key,
            "1.5");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Metafieldable.Key.TooLong");
        entity.Metafields.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateMetafield_TooLongValue_ShouldFailValueTooLong()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        var value = new string(
            'v',
            MetafieldableConstant.Constraints.Value.MaxLength + 1);

        // Act:
        var result = MetafieldableValidator.ValidateMetafield(
            entity,
            "specs",
            "weight",
            value);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Metafieldable.Value.TooLong");
        entity.Metafields.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateMetafield_NullValue_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = MetafieldableValidator.ValidateMetafield(
            entity,
            "specs",
            "weight",
            null);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateMetafield_TooManyMetafields_ShouldFailMetafieldsTooMany()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        for (var i = 0; i < MetafieldableConstant.Constraints.Collection.MaxMetafields; i++)
        {
            entity.Metafields.Add(new Metafield
            {
                Namespace = "ns",
                Key = $"key{i}",
                Type = MetafieldType.ShortText,
                Value = "v"
            });
        }

        // Act:
        var result = MetafieldableValidator.ValidateMetafield(
            entity,
            "ns",
            "new_key",
            "v");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Metafieldable.Metafield.TooMany");
        entity.Metafields.Count.ShouldBe(
            MetafieldableConstant.Constraints.Collection.MaxMetafields);
    }

    [Fact]
    public void ValidateMetafield_UpdateExistingWhenFull_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        for (var i = 0; i < MetafieldableConstant.Constraints.Collection.MaxMetafields; i++)
        {
            entity.Metafields.Add(new Metafield
            {
                Namespace = "ns",
                Key = $"key{i}",
                Type = MetafieldType.ShortText,
                Value = "v"
            });
        }

        // Act:
        var result = MetafieldableValidator.ValidateMetafield(
            entity,
            "ns",
            "key0",
            "updated");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateMetafieldRemoval_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = MetafieldableValidator.ValidateMetafieldRemoval(
            (TestMetafieldable)null!,
            "specs",
            "weight");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Metafieldable.Entity.Required");
    }

    [Fact]
    public void ValidateMetafieldRemoval_BlankNamespace_ShouldFailNamespaceRequired()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = MetafieldableValidator.ValidateMetafieldRemoval(
            entity,
            "   ",
            "weight");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Metafieldable.Namespace.Required");
    }

    [Fact]
    public void ValidateMetafieldRemoval_BlankKey_ShouldFailKeyRequired()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = MetafieldableValidator.ValidateMetafieldRemoval(
            entity,
            "specs",
            "   ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Metafieldable.Key.Required");
    }

    [Fact]
    public void ValidateClearMetafields_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = MetafieldableValidator.ValidateClearMetafields(
            (TestMetafieldable)null!);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Metafieldable.Entity.Required");
    }

    [Fact]
    public void ValidateClearMetafields_ExistingEntity_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        entity.Metafields.Add(new Metafield
        {
            Namespace = "specs",
            Key = "weight",
            Type = MetafieldType.Number,
            Value = "1.5"
        });

        // Act:
        var result = MetafieldableValidator.ValidateClearMetafields(entity);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        // Arrange:
        // Act:
        // Assert:
        // Note: NamespaceTooShort/KeyTooShort are shadowed by Required for empty input
        // (MinLength 1), so their codes are asserted here rather than via the validator.
        MetafieldableResult.Failure.EntityRequired.Code.ShouldBe("Metafieldable.Entity.Required");
        MetafieldableResult.Failure.NamespaceRequired.Code.ShouldBe("Metafieldable.Namespace.Required");
        MetafieldableResult.Failure.NamespaceTooShort.Code.ShouldBe("Metafieldable.Namespace.TooShort");
        MetafieldableResult.Failure.NamespaceTooLong.Code.ShouldBe("Metafieldable.Namespace.TooLong");
        MetafieldableResult.Failure.NamespaceInvalid.Code.ShouldBe("Metafieldable.Namespace.Invalid");
        MetafieldableResult.Failure.KeyRequired.Code.ShouldBe("Metafieldable.Key.Required");
        MetafieldableResult.Failure.KeyTooShort.Code.ShouldBe("Metafieldable.Key.TooShort");
        MetafieldableResult.Failure.KeyTooLong.Code.ShouldBe("Metafieldable.Key.TooLong");
        MetafieldableResult.Failure.KeyInvalid.Code.ShouldBe("Metafieldable.Key.Invalid");
        MetafieldableResult.Failure.ValueTooLong.Code.ShouldBe("Metafieldable.Value.TooLong");
        MetafieldableResult.Failure.MetafieldNotFound.Code.ShouldBe("Metafieldable.Metafield.NotFound");
        MetafieldableResult.Failure.MetafieldsTooMany.Code.ShouldBe("Metafieldable.Metafield.TooMany");
    }

    #endregion

    #region Constant Specs

    [Fact]
    public void Constants_ShouldMatchDocumentedValues()
    {
        // Arrange:
        // Act:
        // Assert:
        MetafieldableConstant.Constraints.Namespace.MinLength.ShouldBe(1);
        MetafieldableConstant.Constraints.Namespace.MaxLength.ShouldBe(64);
        MetafieldableConstant.Constraints.Key.MinLength.ShouldBe(1);
        MetafieldableConstant.Constraints.Key.MaxLength.ShouldBe(64);
        MetafieldableConstant.Constraints.Value.MaxLength.ShouldBe(8000);
        MetafieldableConstant.Constraints.Collection.MaxMetafields.ShouldBe(100);
        MetafieldableConstant.Defaults.Type.Default.ShouldBe(MetafieldType.ShortText);
        MetafieldableConstant.Patterns.SnakeCase.ShouldBe(@"^[a-z0-9_]+$");
        MetafieldableConstant.Patterns.KebabCase.ShouldBe(@"^[a-z0-9]+(?:-[a-z0-9]+)*$");
    }

    #endregion
}
