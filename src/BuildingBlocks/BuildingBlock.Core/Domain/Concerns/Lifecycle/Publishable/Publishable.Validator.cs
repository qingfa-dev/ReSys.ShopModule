using System.Text.RegularExpressions;

using BuildingBlock.Monad.Results;

namespace BuildingBlock.Core.Domain.Concerns.Lifecycle.Publishable;

/// <summary>Validates publication and unpublication operations for <see cref="IPublishable"/> entities.</summary>
public static class PublishableValidator
{
    /// <summary>
    /// Validates the publication metadata that
    /// <see cref="PublishableExtensions.Publish{TValue}"/> is about to apply,
    /// without mutating the entity.
    /// </summary>
    public static Result<TValue> ValidatePublication<TValue>(
        TValue auditable,
        DateTimeOffset nowUtc,
        string? actor,
        bool requireActor = LifecycleConstant.Defaults.RequireActor)
        where TValue : IPublishable
    {
        // Guard: Null entity check.
        if (auditable is null)
        {
            return Result<TValue>.Fail(
                PublishableResult.Failure.EntityRequired);
        }

        // Compute: Chain validation checks in priority order.
        return Result<TValue>.Success(auditable)
            .Ensure(
                predicate: _ => nowUtc != default,
                error: PublishableResult.Failure.PublishedAtRequired)

            .Ensure(
                predicate: _ => nowUtc <= DateTimeOffset.UtcNow.AddSeconds(
                    LifecycleConstant.Constraints.Timestamp.MaxFutureSkewSeconds),
                error: PublishableResult.Failure.PublishedAtInFuture)

            .Ensure(
                predicate: entity => !entity.IsPublished,
                error: PublishableResult.Failure.AlreadyPublished)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    actor.Length <=
                    LifecycleConstant.Constraints.Actor.MaxLength,
                error: PublishableResult.Failure.PublishedByTooLong)

            .Ensure(
                predicate: _ =>
                    !requireActor ||
                    !string.IsNullOrWhiteSpace(actor),
                error: PublishableResult.Failure.PublishedByRequired)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    Regex.IsMatch(actor, LifecycleConstant.Patterns.Actor),
                error: PublishableResult.Failure.ActorInvalid)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    actor.Length >=
                    LifecycleConstant.Constraints.Actor.MinLength,
                error: PublishableResult.Failure.PublishedByTooShort);
    }

    /// <summary>
    /// Validates the unpublication metadata that
    /// <see cref="PublishableExtensions.Unpublish{TValue}"/> is about to apply,
    /// without mutating the entity.
    /// </summary>
    public static Result<TValue> ValidateUnpublication<TValue>(
        TValue auditable,
        string? actor,
        bool requireActor = LifecycleConstant.Defaults.RequireActor)
        where TValue : IPublishable
    {
        // Guard: Null entity check.
        if (auditable is null)
        {
            return Result<TValue>.Fail(
                PublishableResult.Failure.EntityRequired);
        }

        // Compute: Chain validation checks in priority order.
        return Result<TValue>.Success(auditable)
            .Ensure(
                predicate: entity => entity.IsPublished,
                error: PublishableResult.Failure.NotPublished)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    actor.Length <=
                    LifecycleConstant.Constraints.Actor.MaxLength,
                error: PublishableResult.Failure.PublishedByTooLong)

            .Ensure(
                predicate: _ =>
                    !requireActor ||
                    !string.IsNullOrWhiteSpace(actor),
                error: PublishableResult.Failure.PublishedByRequired)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    Regex.IsMatch(actor, LifecycleConstant.Patterns.Actor),
                error: PublishableResult.Failure.ActorInvalid)

            .Ensure(
                predicate: _ =>
                    actor is null ||
                    actor.Length >=
                    LifecycleConstant.Constraints.Actor.MinLength,
                error: PublishableResult.Failure.PublishedByTooShort);
    }
}
