using System.Text.RegularExpressions;

using BuildingBlock.Monad.Results;

namespace BuildingBlock.Core.Domain.Concerns.Content.Sluggable;

/// <summary>Validator for the <see cref="ISluggable"/> concern.</summary>
public static class SluggableValidator
{
    #region Public Methods

    /// <summary>
    /// Validates the slug that <see cref="SluggableExtensions.SetSlug{TValue}"/>
    /// is about to apply, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the sluggable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="slug">The slug to validate.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateSlug<TValue>(
        TValue auditable,
        string? slug)
        where TValue : ISluggable
    {
        // Guard: Entity must not be null.
        if (auditable is null)
        {
            return Result<TValue>.Fail(
                SluggableResult.Failure.EntityRequired);
        }

        return Result<TValue>.Success(auditable)
            .Ensure(
                predicate: _ => !string.IsNullOrWhiteSpace(slug),
                error: SluggableResult.Failure.SlugRequired)

            .Ensure(
                predicate: _ =>
                    slug!.Length >= SluggableConstant.Constraints.Slug.MinLength,
                error: SluggableResult.Failure.SlugTooShort)

            .Ensure(
                predicate: _ =>
                    slug!.Length <= SluggableConstant.Constraints.Slug.MaxLength,
                error: SluggableResult.Failure.SlugTooLong)

            .Ensure(
                predicate: _ =>
                    Regex.IsMatch(slug!, SluggableConstant.Patterns.Slug),
                error: SluggableResult.Failure.SlugInvalid);
    }

    #endregion
}
