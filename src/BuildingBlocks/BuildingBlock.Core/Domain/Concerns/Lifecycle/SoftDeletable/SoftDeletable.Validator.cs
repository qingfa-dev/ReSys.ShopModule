using System.Text.RegularExpressions;

using BuildingBlock.Monad.Results;

namespace BuildingBlock.Core.Domain.Concerns.Lifecycle.SoftDeletable;

/// <summary>Validates deletion and restoration operations for <see cref="ISoftDeletable"/> entities.</summary>
public static class SoftDeletableValidator
{
    /// <summary>
    /// Validates the deletion metadata that
    /// <see cref="SoftDeletableExtensions.MarkDeleted{TValue}"/> is about to apply,
    /// without mutating the entity.
    /// </summary>
    public static Result<TValue> ValidateDeletion<TValue>(
        TValue auditable,
        DateTimeOffset nowUtc,
        string? actor,
        bool requireActor = LifecycleConstant.Defaults.RequireActor)
        where TValue : ISoftDeletable
    {
        // Guard: Null entity check.
        if (auditable is null)
        {
            return Result<TValue>.Fail(
                SoftDeletableResult.Failure.EntityRequired);
        }

        // Compute: Chain validation checks in priority order.
        return Result<TValue>.Success(auditable)
            .Ensure(
                predicate: _ => nowUtc != default,
                error: SoftDeletableResult.Failure.DeletedAtRequired)

            .Ensure(
                predicate: _ => nowUtc <= DateTimeOffset.UtcNow.AddSeconds(
                    LifecycleConstant.Constraints.Timestamp.MaxFutureSkewSeconds),
                error: SoftDeletableResult.Failure.DeletedAtInFuture)

            .Ensure(
                predicate: entity => !entity.IsDeleted,
                error: SoftDeletableResult.Failure.AlreadyDeleted)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    actor.Length <=
                    LifecycleConstant.Constraints.Actor.MaxLength,
                error: SoftDeletableResult.Failure.DeletedByTooLong)

            .Ensure(
                predicate: _ =>
                    !requireActor ||
                    !string.IsNullOrWhiteSpace(actor),
                error: SoftDeletableResult.Failure.DeletedByRequired)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    Regex.IsMatch(actor, LifecycleConstant.Patterns.Actor),
                error: SoftDeletableResult.Failure.ActorInvalid)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    actor.Length >=
                    LifecycleConstant.Constraints.Actor.MinLength,
                error: SoftDeletableResult.Failure.DeletedByTooShort);
    }

    /// <summary>
    /// Validates the restoration metadata that
    /// <see cref="SoftDeletableExtensions.Restore{TValue}"/> is about to apply,
    /// without mutating the entity.
    /// </summary>
    public static Result<TValue> ValidateRestoration<TValue>(
        TValue auditable,
        string? actor,
        bool requireActor = LifecycleConstant.Defaults.RequireActor)
        where TValue : ISoftDeletable
    {
        // Guard: Null entity check.
        if (auditable is null)
        {
            return Result<TValue>.Fail(
                SoftDeletableResult.Failure.EntityRequired);
        }

        // Compute: Chain validation checks in priority order.
        return Result<TValue>.Success(auditable)
            .Ensure(
                predicate: entity => entity.IsDeleted,
                error: SoftDeletableResult.Failure.NotDeleted)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    actor.Length <=
                    LifecycleConstant.Constraints.Actor.MaxLength,
                error: SoftDeletableResult.Failure.DeletedByTooLong)

            .Ensure(
                predicate: _ =>
                    !requireActor ||
                    !string.IsNullOrWhiteSpace(actor),
                error: SoftDeletableResult.Failure.DeletedByRequired)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    Regex.IsMatch(actor, LifecycleConstant.Patterns.Actor),
                error: SoftDeletableResult.Failure.ActorInvalid)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    actor.Length >=
                    LifecycleConstant.Constraints.Actor.MinLength,
                error: SoftDeletableResult.Failure.DeletedByTooShort);
    }
}
