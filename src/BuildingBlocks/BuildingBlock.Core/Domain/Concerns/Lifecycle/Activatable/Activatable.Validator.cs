using System.Text.RegularExpressions;

using BuildingBlock.Monad.Results;

namespace BuildingBlock.Core.Domain.Concerns.Lifecycle.Activatable;

/// <summary>Validates activation and deactivation operations for <see cref="IActivatable"/> entities.</summary>
public static class ActivatableValidator
{
    /// <summary>
    /// Validates the activation metadata that
    /// <see cref="ActivatableExtensions.Activate{TValue}"/> is about to apply,
    /// without mutating the entity.
    /// </summary>
    public static Result<TValue> ValidateActivation<TValue>(
        TValue auditable,
        DateTimeOffset nowUtc,
        string? actor,
        bool requireActor = LifecycleConstant.Defaults.RequireActor)
        where TValue : IActivatable
    {
        // Guard: Null entity check.
        if (auditable is null)
        {
            return Result<TValue>.Fail(
                ActivatableResult.Failure.EntityRequired);
        }

        // Compute: Chain validation checks in priority order.
        return Result<TValue>.Success(auditable)
            .Ensure(
                predicate: _ => nowUtc != default,
                error: ActivatableResult.Failure.ActivatedAtRequired)

            .Ensure(
                predicate: _ => nowUtc <= DateTimeOffset.UtcNow.AddSeconds(
                    LifecycleConstant.Constraints.Timestamp.MaxFutureSkewSeconds),
                error: ActivatableResult.Failure.ActivatedAtInFuture)

            .Ensure(
                predicate: entity => !entity.IsActive,
                error: ActivatableResult.Failure.AlreadyActive)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    actor.Length <=
                    LifecycleConstant.Constraints.Actor.MaxLength,
                error: ActivatableResult.Failure.ActivatedByTooLong)

            .Ensure(
                predicate: _ =>
                    !requireActor ||
                    !string.IsNullOrWhiteSpace(actor),
                error: ActivatableResult.Failure.ActivatedByRequired)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    Regex.IsMatch(actor, LifecycleConstant.Patterns.Actor),
                error: ActivatableResult.Failure.ActorInvalid)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    actor.Length >=
                    LifecycleConstant.Constraints.Actor.MinLength,
                error: ActivatableResult.Failure.ActivatedByTooShort);
    }

    /// <summary>
    /// Validates the deactivation metadata that
    /// <see cref="ActivatableExtensions.Deactivate{TValue}"/> is about to apply,
    /// without mutating the entity.
    /// </summary>
    public static Result<TValue> ValidateDeactivation<TValue>(
        TValue auditable,
        string? actor,
        bool requireActor = LifecycleConstant.Defaults.RequireActor)
        where TValue : IActivatable
    {
        // Guard: Null entity check.
        if (auditable is null)
        {
            return Result<TValue>.Fail(
                ActivatableResult.Failure.EntityRequired);
        }

        // Compute: Chain validation checks in priority order.
        return Result<TValue>.Success(auditable)
            .Ensure(
                predicate: entity => entity.IsActive,
                error: ActivatableResult.Failure.NotActive)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    actor.Length <=
                    LifecycleConstant.Constraints.Actor.MaxLength,
                error: ActivatableResult.Failure.ActivatedByTooLong)

            .Ensure(
                predicate: _ =>
                    !requireActor ||
                    !string.IsNullOrWhiteSpace(actor),
                error: ActivatableResult.Failure.ActivatedByRequired)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    Regex.IsMatch(actor, LifecycleConstant.Patterns.Actor),
                error: ActivatableResult.Failure.ActorInvalid)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    actor.Length >=
                    LifecycleConstant.Constraints.Actor.MinLength,
                error: ActivatableResult.Failure.ActivatedByTooShort);
    }
}
