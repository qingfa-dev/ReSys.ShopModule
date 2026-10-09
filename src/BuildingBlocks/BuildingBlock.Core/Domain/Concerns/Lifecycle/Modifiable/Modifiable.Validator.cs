using System.Text.RegularExpressions;

using BuildingBlock.Core.Domain.Concerns.Lifecycle.Creatable;

using BuildingBlock.Monad.Results;

namespace BuildingBlock.Core.Domain.Concerns.Lifecycle.Modifiable;

/// <summary>Validates modification operations for <see cref="ICreatable"/> and <see cref="IModifiable"/> entities.</summary>
public static class ModifiableValidator
{
    /// <summary>
    /// Validates the audit metadata that <see cref="ModifiableExtensions.MarkModified{TValue}"/>
    /// is about to apply, without mutating the entity.
    /// </summary>
    public static Result<TValue> ValidateModification<TValue>(
        TValue auditable,
        DateTimeOffset nowUtc,
        string? actor,
        bool requireModifiedBy = LifecycleConstant.Defaults.RequireActor)
        where TValue : ICreatable, IModifiable
    {
        // Guard: Null entity check.
        if (auditable is null)
        {
            return Result<TValue>.Fail(
                ModifiableResult.Failure.EntityRequired);
        }

        // Compute: Chain validation checks in priority order.
        return Result<TValue>.Success(auditable)
            .Ensure(
                predicate: _ => nowUtc != default,
                error: ModifiableResult.Failure.ModifiedAtRequired)

            .Ensure(
                predicate: _ => nowUtc <= DateTimeOffset.UtcNow.AddSeconds(
                    LifecycleConstant.Constraints.Timestamp.MaxFutureSkewSeconds),
                error: ModifiableResult.Failure.ModifiedAtInFuture)

            .Ensure(
                predicate: entity => entity.CreatedAtUtc != default,
                error: ModifiableResult.Failure.NotInitialized)

            .Ensure(
                predicate: entity => nowUtc >= entity.CreatedAtUtc,
                error: ModifiableResult.Failure.ModifiedAtBeforeCreatedAt)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    actor.Length <=
                    LifecycleConstant.Constraints.Actor.MaxLength,
                error: ModifiableResult.Failure.ModifiedByTooLong)

            .Ensure(
                predicate: _ =>
                    !requireModifiedBy ||
                    !string.IsNullOrWhiteSpace(actor),
                error: ModifiableResult.Failure.ModifiedByRequired)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    Regex.IsMatch(actor, LifecycleConstant.Patterns.Actor),
                error: ModifiableResult.Failure.ActorInvalid)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    actor.Length >=
                    LifecycleConstant.Constraints.Actor.MinLength,
                error: ModifiableResult.Failure.ModifiedByTooShort);
    }
}
