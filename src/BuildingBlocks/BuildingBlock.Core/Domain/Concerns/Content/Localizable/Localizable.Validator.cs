using System.Text.RegularExpressions;

using BuildingBlock.Monad.Results;

namespace BuildingBlock.Core.Domain.Concerns.Content.Localizable;

/// <summary>Validator for the <see cref="ILocalizable"/> concern.</summary>
public static class LocalizableValidator
{
    #region Public Methods

    /// <summary>
    /// Validates the localized value that
    /// <see cref="LocalizableExtensions.SetLocalizedValue{TValue}"/>
    /// is about to apply, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the localizable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="property">The localized property name.</param>
    /// <param name="locale">The locale identifier.</param>
    /// <param name="value">The localized value.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateLocalizedValue<TValue>(
        TValue auditable,
        string? property,
        string? locale,
        string? value)
        where TValue : ILocalizable
    {
        // Guard: Entity must not be null.
        if (auditable is null)
        {
            return Result<TValue>.Fail(
                LocalizableResult.Failure.EntityRequired);
        }

        return Result<TValue>.Success(auditable)
            .Ensure(
                predicate: _ => !string.IsNullOrWhiteSpace(property),
                error: LocalizableResult.Failure.PropertyRequired)

            .Ensure(
                predicate: _ =>
                    property!.Length >=
                    LocalizableConstant.Constraints.Property.MinLength,
                error: LocalizableResult.Failure.PropertyTooShort)

            .Ensure(
                predicate: _ =>
                    property!.Length <=
                    LocalizableConstant.Constraints.Property.MaxLength,
                error: LocalizableResult.Failure.PropertyTooLong)

            .Ensure(
                predicate: _ => !string.IsNullOrWhiteSpace(locale),
                error: LocalizableResult.Failure.LocaleRequired)

            .Ensure(
                predicate: _ =>
                    locale!.Length >=
                    LocalizableConstant.Constraints.Locale.MinLength,
                error: LocalizableResult.Failure.LocaleTooShort)

            .Ensure(
                predicate: _ =>
                    locale!.Length <=
                    LocalizableConstant.Constraints.Locale.MaxLength,
                error: LocalizableResult.Failure.LocaleTooLong)

            .Ensure(
                predicate: _ =>
                    Regex.IsMatch(locale!, LocalizableConstant.Patterns.Locale),
                error: LocalizableResult.Failure.LocaleInvalid)

            .Ensure(
                predicate: _ =>
                    value is null ||
                    value.Length <=
                    LocalizableConstant.Constraints.Value.MaxLength,
                error: LocalizableResult.Failure.ValueTooLong)

            .Ensure(
                predicate: entity =>
                    ContainsTranslation(entity, property!, locale!) ||
                    entity.LocalizedValues.Count <
                    LocalizableConstant.Constraints.Collection.MaxTranslations,
                error: LocalizableResult.Failure.TranslationsTooMany);
    }

    /// <summary>
    /// Validates that the localized value that
    /// <see cref="LocalizableExtensions.RemoveLocalizedValue{TValue}"/>
    /// targets is present, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the localizable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="property">The localized property name.</param>
    /// <param name="locale">The locale identifier.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateLocalizedValueRemoval<TValue>(
        TValue auditable,
        string? property,
        string? locale)
        where TValue : ILocalizable
    {
        // Guard: Entity must not be null.
        if (auditable is null)
        {
            return Result<TValue>.Fail(
                LocalizableResult.Failure.EntityRequired);
        }

        return Result<TValue>.Success(auditable)
            .Ensure(
                predicate: _ => !string.IsNullOrWhiteSpace(property),
                error: LocalizableResult.Failure.PropertyRequired)

            .Ensure(
                predicate: _ => !string.IsNullOrWhiteSpace(locale),
                error: LocalizableResult.Failure.LocaleRequired)

            .Ensure(
                predicate: entity =>
                    ContainsTranslation(entity, property!, locale!),
                error: LocalizableResult.Failure.ValueNotFound);
    }

    /// <summary>
    /// Validates that the entity that
    /// <see cref="LocalizableExtensions.ClearLocalizedValues{TValue}"/>
    /// is about to clear exists, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the localizable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateClearLocalizedValues<TValue>(
        TValue auditable)
        where TValue : ILocalizable
    {
        // Guard: Entity must not be null.
        if (auditable is null)
        {
            return Result<TValue>.Fail(
                LocalizableResult.Failure.EntityRequired);
        }

        return Result<TValue>.Success(auditable);
    }

    #endregion

    #region Private Methods

    /// <summary>Checks whether a (property, locale) translation already exists.</summary>
    private static bool ContainsTranslation<TValue>(
        TValue auditable,
        string property,
        string locale)
        where TValue : ILocalizable
    {
        return auditable.LocalizedValues.Any(entry =>
            entry.Property == property &&
            entry.Locale == locale);
    }

    #endregion
}
