using BuildingBlock.Core.Domain.Concerns.Content.Localizable;

namespace BuildingBlock.Core.Domain.UnitTest.Concerns.Content.Localizable;

/// <summary>Spec for <see cref="LocalizableExtensions"/> covering extension behavior,
/// <see cref="LocalizableValidator"/> branches and <see cref="LocalizableConstant"/> values.</summary>
public class LocalizableExtensionSpec
{
    #region Helper Methods

    private sealed class TestLocalizable : ILocalizable
    {
        public ICollection<LocalizedValue> LocalizedValues { get; } =
            new List<LocalizedValue>();
    }

    /// <summary>Creates a successful result wrapping the test entity.</summary>
    /// <param name="entity">The test entity.</param>
    /// <returns>A successful result.</returns>
    private static Result<TestLocalizable> Success(
        TestLocalizable entity)
    {
        return Result<TestLocalizable>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void SetLocalizedValue_NewEntry_ShouldAddEntry()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = Success(entity)
            .SetLocalizedValue("title", "en", "Hello");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.LocalizedValues.ShouldHaveSingleItem();
        entity.LocalizedValues.Single().Property.ShouldBe("title");
        entity.LocalizedValues.Single().Locale.ShouldBe("en");
        entity.LocalizedValues.Single().Value.ShouldBe("Hello");
    }

    [Fact]
    public void SetLocalizedValue_ExistingEntry_ShouldUpdateValue()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Old");

        // Act:
        var result = Success(entity)
            .SetLocalizedValue("title", "en", "New");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.LocalizedValues.ShouldHaveSingleItem();
        entity.LocalizedValues.Single().Value.ShouldBe("New");
    }

    [Fact]
    public void SetLocalizedValue_NullProperty_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = Success(entity)
            .SetLocalizedValue(null, "en", "Hello");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Localizable.Property.Required");
        entity.LocalizedValues.ShouldBeEmpty();
    }

    [Fact]
    public void SetLocalizedValue_TooLongProperty_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestLocalizable();
        var property = new string(
            'p',
            LocalizableConstant.Constraints.Property.MaxLength + 1);

        // Act:
        var result = Success(entity)
            .SetLocalizedValue(property, "en", "Hello");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Localizable.Property.TooLong");
        entity.LocalizedValues.ShouldBeEmpty();
    }

    [Fact]
    public void SetLocalizedValue_NullLocale_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = Success(entity)
            .SetLocalizedValue("title", null, "Hello");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Localizable.Locale.Required");
        entity.LocalizedValues.ShouldBeEmpty();
    }

    [Fact]
    public void SetLocalizedValue_TooLongLocale_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestLocalizable();
        var locale = new string(
            'l',
            LocalizableConstant.Constraints.Locale.MaxLength + 1);

        // Act:
        var result = Success(entity)
            .SetLocalizedValue("title", locale, "Hello");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Localizable.Locale.TooLong");
        entity.LocalizedValues.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateLocalizedValue_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = LocalizableValidator.ValidateLocalizedValue(
            (TestLocalizable)null!,
            "title",
            "en",
            "Hello");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Localizable.Entity.Required");
    }

    [Fact]
    public void RemoveLocalizedValue_ExistingEntry_ShouldRemoveEntry()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Hello");

        // Act:
        var result = Success(entity)
            .RemoveLocalizedValue("title", "en");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.LocalizedValues.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveLocalizedValue_MissingEntry_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Hello");

        // Act:
        var result = Success(entity)
            .RemoveLocalizedValue("title", "fr");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Localizable.Value.NotFound");
        entity.LocalizedValues.ShouldHaveSingleItem();
    }

    [Fact]
    public void ClearLocalizedValues_WithEntries_ShouldClearEntries()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Hello");
        Success(entity).SetLocalizedValue("title", "fr", "Bonjour");

        // Act:
        var result = Success(entity).ClearLocalizedValues();

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.LocalizedValues.ShouldBeEmpty();
    }

    [Fact]
    public void ClearLocalizedValues_EmptyCollection_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = Success(entity).ClearLocalizedValues();

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.LocalizedValues.ShouldBeEmpty();
    }

    [Fact]
    public void SetLocalizedValue_NullValue_ShouldSucceedWithNullStored()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = Success(entity).SetLocalizedValue("title", "en", null);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        var entry = entity.LocalizedValues.ShouldHaveSingleItem();
        entry.Value.ShouldBeNull();
    }

    [Fact]
    public void SetLocalizedValue_PaddedInputs_ShouldNormalizeBeforeStore()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = Success(entity).SetLocalizedValue("  title  ", "  en  ", "Hello");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        var entry = entity.LocalizedValues.ShouldHaveSingleItem();
        entry.Property.ShouldBe("title");
        entry.Locale.ShouldBe("en");
    }

    [Fact]
    public void SetLocalizedValue_UpdateExistingWhenFull_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestLocalizable();
        for (var i = 0; i < LocalizableConstant.Constraints.Collection.MaxTranslations; i++)
        {
            entity.LocalizedValues.Add(new LocalizedValue
            {
                Property = $"prop{i}",
                Locale = "en",
                Value = "Hello"
            });
        }

        // Act:
        var result = Success(entity).SetLocalizedValue("prop0", "en", "Updated");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.LocalizedValues.Count.ShouldBe(
            LocalizableConstant.Constraints.Collection.MaxTranslations);
        entity.LocalizedValues.Single(e => e.Property == "prop0").Value.ShouldBe("Updated");
    }

    [Fact]
    public void SetLocalizedValue_InputFailure_ShouldPropagateWithoutMutation()
    {
        // Arrange:
        var entity = new TestLocalizable();
        var input = Result<TestLocalizable>.Fail(
            Error.UnprocessableEntity("Test.Fail", "boom"));

        // Act:
        var result = input.SetLocalizedValue("title", "en", "Hello");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Test.Fail");
        entity.LocalizedValues.ShouldBeEmpty();
    }

    [Fact]
    public void GetLocalizedValue_ExistingEntry_ShouldReturnValue()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Hello");

        // Act:
        var value = entity.GetLocalizedValue("title", "en");

        // Assert:
        value.ShouldBe("Hello");
    }

    [Fact]
    public void GetLocalizedValue_MissingEntry_ShouldReturnNull()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Hello");

        // Act:
        var value = entity.GetLocalizedValue("title", "fr");

        // Assert:
        value.ShouldBeNull();
    }

    [Fact]
    public void GetLocalizedValue_NullEntity_ShouldReturnNull()
    {
        // Arrange:
        TestLocalizable entity = null!;

        // Act:
        var value = entity.GetLocalizedValue("title", "en");

        // Assert:
        value.ShouldBeNull();
    }

    [Fact]
    public void GetLocalizedValue_NullProperty_ShouldReturnNull()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Hello");

        // Act:
        var value = entity.GetLocalizedValue(null, "en");

        // Assert:
        value.ShouldBeNull();
    }

    [Fact]
    public void GetLocalizedValue_NullLocale_ShouldReturnNull()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Hello");

        // Act:
        var value = entity.GetLocalizedValue("title", null);

        // Assert:
        value.ShouldBeNull();
    }

    [Fact]
    public void GetLocalizedValue_PaddedInputs_ShouldMatchNormalized()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Hello");

        // Act:
        var value = entity.GetLocalizedValue("  title  ", "  en  ");

        // Assert:
        value.ShouldBe("Hello");
    }

    [Fact]
    public void NormalizeProperty_Null_ShouldReturnNull()
    {
        // Arrange:
        // Act:
        var normalized = LocalizableExtensions.NormalizeProperty(null);

        // Assert:
        normalized.ShouldBeNull();
    }

    [Fact]
    public void NormalizeProperty_Padded_ShouldTrim()
    {
        // Arrange:
        // Act:
        var normalized = LocalizableExtensions.NormalizeProperty("  title  ");

        // Assert:
        normalized.ShouldBe("title");
    }

    [Fact]
    public void NormalizeLocale_Null_ShouldReturnNull()
    {
        // Arrange:
        // Act:
        var normalized = LocalizableExtensions.NormalizeLocale(null);

        // Assert:
        normalized.ShouldBeNull();
    }

    [Fact]
    public void NormalizeLocale_Padded_ShouldTrim()
    {
        // Arrange:
        // Act:
        var normalized = LocalizableExtensions.NormalizeLocale("  en  ");

        // Assert:
        normalized.ShouldBe("en");
    }

    #endregion

    #region Validator Specs

    [Fact]
    public void ValidateLocalizedValue_BlankProperty_ShouldFailPropertyRequired()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValue(
            entity,
            "   ",
            "en",
            "Hello");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Localizable.Property.Required");
        entity.LocalizedValues.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateLocalizedValue_BlankLocale_ShouldFailLocaleRequired()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValue(
            entity,
            "title",
            "   ",
            "Hello");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Localizable.Locale.Required");
        entity.LocalizedValues.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateLocalizedValue_ShortLocale_ShouldFailLocaleTooShort()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValue(
            entity,
            "title",
            "a",
            "Hello");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Localizable.Locale.TooShort");
        entity.LocalizedValues.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateLocalizedValue_InvalidLocale_ShouldFailLocaleInvalid()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValue(
            entity,
            "title",
            "en_us!",
            "Hello");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Localizable.Locale.Invalid");
        entity.LocalizedValues.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateLocalizedValue_TooLongValue_ShouldFailValueTooLong()
    {
        // Arrange:
        var entity = new TestLocalizable();
        var value = new string(
            'v',
            LocalizableConstant.Constraints.Value.MaxLength + 1);

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValue(
            entity,
            "title",
            "en",
            value);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Localizable.Value.TooLong");
        entity.LocalizedValues.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateLocalizedValue_TooManyTranslations_ShouldFailTranslationsTooMany()
    {
        // Arrange:
        var entity = new TestLocalizable();
        for (var i = 0; i < LocalizableConstant.Constraints.Collection.MaxTranslations; i++)
        {
            entity.LocalizedValues.Add(new LocalizedValue
            {
                Property = $"prop{i}",
                Locale = "en",
                Value = "Hello"
            });
        }

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValue(
            entity,
            "new-prop",
            "en",
            "Hello");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Localizable.Translations.TooMany");
        entity.LocalizedValues.Count.ShouldBe(
            LocalizableConstant.Constraints.Collection.MaxTranslations);
    }

    [Fact]
    public void ValidateLocalizedValue_UpdateExistingWhenFull_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestLocalizable();
        for (var i = 0; i < LocalizableConstant.Constraints.Collection.MaxTranslations; i++)
        {
            entity.LocalizedValues.Add(new LocalizedValue
            {
                Property = $"prop{i}",
                Locale = "en",
                Value = "Hello"
            });
        }

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValue(
            entity,
            "prop0",
            "en",
            "Updated");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateLocalizedValueRemoval_MissingEntry_ShouldFailValueNotFound()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValueRemoval(
            entity,
            "title",
            "en");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Localizable.Value.NotFound");
    }

    [Fact]
    public void ValidateLocalizedValueRemoval_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = LocalizableValidator.ValidateLocalizedValueRemoval(
            (TestLocalizable)null!,
            "title",
            "en");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Localizable.Entity.Required");
    }

    [Fact]
    public void ValidateClearLocalizedValues_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = LocalizableValidator.ValidateClearLocalizedValues(
            (TestLocalizable)null!);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Localizable.Entity.Required");
    }

    [Fact]
    public void ValidateClearLocalizedValues_ExistingEntity_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestLocalizable();
        entity.LocalizedValues.Add(new LocalizedValue
        {
            Property = "title",
            Locale = "en",
            Value = "Hello"
        });

        // Act:
        var result = LocalizableValidator.ValidateClearLocalizedValues(entity);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateLocalizedValue_NullValue_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValue(
            entity,
            "title",
            "en",
            null);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateLocalizedValueRemoval_ExistingEntry_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestLocalizable();
        entity.LocalizedValues.Add(new LocalizedValue
        {
            Property = "title",
            Locale = "en",
            Value = "Hello"
        });

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValueRemoval(
            entity,
            "title",
            "en");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        // Arrange:
        // Act:
        // Assert:
        // Note: PropertyTooShort is shadowed by PropertyRequired for empty input
        // (MinLength 1), so its code is asserted here rather than via the validator.
        LocalizableResult.Failure.EntityRequired.Code.ShouldBe("Localizable.Entity.Required");
        LocalizableResult.Failure.PropertyRequired.Code.ShouldBe("Localizable.Property.Required");
        LocalizableResult.Failure.PropertyTooShort.Code.ShouldBe("Localizable.Property.TooShort");
        LocalizableResult.Failure.PropertyTooLong.Code.ShouldBe("Localizable.Property.TooLong");
        LocalizableResult.Failure.LocaleRequired.Code.ShouldBe("Localizable.Locale.Required");
        LocalizableResult.Failure.LocaleTooShort.Code.ShouldBe("Localizable.Locale.TooShort");
        LocalizableResult.Failure.LocaleTooLong.Code.ShouldBe("Localizable.Locale.TooLong");
        LocalizableResult.Failure.LocaleInvalid.Code.ShouldBe("Localizable.Locale.Invalid");
        LocalizableResult.Failure.ValueTooLong.Code.ShouldBe("Localizable.Value.TooLong");
        LocalizableResult.Failure.ValueNotFound.Code.ShouldBe("Localizable.Value.NotFound");
        LocalizableResult.Failure.TranslationsTooMany.Code.ShouldBe("Localizable.Translations.TooMany");
    }

    #endregion

    #region Constant Specs

    [Fact]
    public void Constants_ShouldMatchDocumentedValues()
    {
        // Arrange:
        // Act:
        // Assert:
        LocalizableConstant.Constraints.Property.MinLength.ShouldBe(1);
        LocalizableConstant.Constraints.Property.MaxLength.ShouldBe(128);
        LocalizableConstant.Constraints.Locale.MinLength.ShouldBe(2);
        LocalizableConstant.Constraints.Locale.MaxLength.ShouldBe(35);
        LocalizableConstant.Constraints.Value.MaxLength.ShouldBe(4000);
        LocalizableConstant.Constraints.Collection.MaxTranslations.ShouldBe(200);
        LocalizableConstant.Defaults.Locale.ShouldBe("en");
        LocalizableConstant.Patterns.Locale.ShouldBe(@"^[A-Za-z]{2,8}(?:-[A-Za-z0-9]{2,8})*$");
    }

    #endregion
}
