using BuildingBlock.Core.Domain.Concerns.Lifecycle;
using BuildingBlock.Core.Domain.Concerns.Lifecycle.Publishable;
namespace BuildingBlock.Core.Domain.Tests.Concerns.Lifecycle.Publishable;

/// <summary>Specification tests for <see cref="PublishableExtensions"/>.</summary>
public class PublishableExtensionSpec
{
    private static readonly DateTimeOffset NowUtc =
        new(2024, 5, 1, 12, 0, 0, TimeSpan.Zero);

    #region Helper Methods

    /// <summary>Creates a successful result wrapping the given entity.</summary>
    private static Result<TestPublishable> Success(TestPublishable entity)
    {
        return Result<TestPublishable>.Success(entity);
    }

    private sealed class TestPublishable : IPublishable
    {
        public bool IsPublished { get; set; }
        public DateTimeOffset? PublishedAtUtc { get; set; }
        public string? PublishedBy { get; set; }
    }

    private static TestPublishable PublishedEntity(
        string? publishedBy = "bob",
        DateTimeOffset? publishedAtUtc = null)
    {
        return new TestPublishable
        {
            IsPublished = true,
            PublishedAtUtc = publishedAtUtc ?? NowUtc,
            PublishedBy = publishedBy
        };
    }

    #endregion

    #region Test Cases

    #region Publish

    [Fact]
    public void Publish_UnpublishedEntity_ShouldSetPublicationFields()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Publish(NowUtc, "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.IsPublished.ShouldBeTrue();
        entity.PublishedAtUtc.ShouldBe(NowUtc);
        entity.PublishedBy.ShouldBe("alice");
    }

    [Fact]
    public void Publish_OffsetTimestamp_ShouldStoreUtcInstant()
    {
        // Arrange:
        var entity = new TestPublishable();
        var offset = new DateTimeOffset(2024, 5, 1, 20, 0, 0, TimeSpan.FromHours(8));

        // Act:
        var result = Success(entity).Publish(offset, "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.PublishedAtUtc!.Value.Offset.ShouldBe(TimeSpan.Zero);
        entity.PublishedAtUtc.ShouldBe(offset.ToUniversalTime());
    }

    [Fact]
    public void Publish_DefaultNowUtc_ShouldFailPublishedAtRequiredWithoutMutation()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Publish(default, "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.PublishedAt.Required");
        entity.IsPublished.ShouldBeFalse();
        entity.PublishedAtUtc.ShouldBeNull();
    }

    [Fact]
    public void Publish_AlreadyPublished_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = Success(entity).Publish(NowUtc, "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.AlreadyPublished");
        entity.PublishedAtUtc.ShouldBe(NowUtc);
        entity.PublishedBy.ShouldBe("bob");
    }

    [Fact]
    public void Publish_ActorTooLong_ShouldFailPublishedByTooLongWithoutMutation()
    {
        // Arrange:
        var entity = new TestPublishable();
        var actor = new string('a', LifecycleConstant.Constraints.MaxActorLength + 1);

        // Act:
        var result = Success(entity).Publish(NowUtc, actor);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.PublishedBy.TooLong");
        entity.IsPublished.ShouldBeFalse();
    }

    [Fact]
    public void Publish_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Publish(NowUtc, " alice ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.Actor.Invalid");
        entity.IsPublished.ShouldBeFalse();
    }

    [Fact]
    public void Publish_EmptyActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Publish(NowUtc, string.Empty);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.Actor.Invalid");
    }

    #endregion

    #region Unpublish

    [Fact]
    public void Unpublish_PublishedEntity_ShouldClearPublicationFields()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = Success(entity).Unpublish("carol");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.IsPublished.ShouldBeFalse();
        entity.PublishedAtUtc.ShouldBeNull();
        entity.PublishedBy.ShouldBeNull();
    }

    [Fact]
    public void Unpublish_NotPublished_ShouldFailNotPublishedWithoutMutation()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Unpublish("carol");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.NotPublished");
        entity.IsPublished.ShouldBeFalse();
    }

    [Fact]
    public void Unpublish_RequireActorWithoutActor_ShouldFailPublishedByRequiredWithoutMutation()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = Success(entity).Unpublish(
            actor: null,
            requireActor: true);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.PublishedBy.Required");
        entity.IsPublished.ShouldBeTrue();
        entity.PublishedAtUtc.ShouldBe(NowUtc);
    }

    #endregion

    #region Validator Specs

    [Fact]
    public void ValidatePublication_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange + Act:
        var result = PublishableValidator.ValidatePublication<TestPublishable>(
            null!,
            NowUtc,
            "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.Entity.Required");
    }

    [Fact]
    public void ValidatePublication_DefaultTimestamp_ShouldFailPublishedAtRequired()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = PublishableValidator.ValidatePublication(
            entity,
            default,
            "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.PublishedAt.Required");
    }

    [Fact]
    public void ValidatePublication_FutureTimestampBeyondSkew_ShouldFailPublishedAtInFuture()
    {
        // Arrange:
        var entity = new TestPublishable();
        var future = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Constraints.Timestamp.MaxFutureSkewSeconds + 60);

        // Act:
        var result = PublishableValidator.ValidatePublication(
            entity,
            future,
            "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.PublishedAt.InFuture");
    }

    [Fact]
    public void ValidatePublication_AlreadyPublished_ShouldFailAlreadyPublished()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = PublishableValidator.ValidatePublication(
            entity,
            NowUtc,
            "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.AlreadyPublished");
    }

    [Fact]
    public void ValidatePublication_ActorTooLong_ShouldFailPublishedByTooLong()
    {
        // Arrange:
        var entity = new TestPublishable();
        var actor = new string('a', LifecycleConstant.Constraints.Actor.MaxLength + 1);

        // Act:
        var result = PublishableValidator.ValidatePublication(
            entity,
            NowUtc,
            actor);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.PublishedBy.TooLong");
    }

    [Fact]
    public void ValidatePublication_RequireActorWithoutActor_ShouldFailPublishedByRequired()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = PublishableValidator.ValidatePublication(
            entity,
            NowUtc,
            actor: null,
            requireActor: true);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.PublishedBy.Required");
    }

    [Fact]
    public void ValidatePublication_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = PublishableValidator.ValidatePublication(
            entity,
            NowUtc,
            " alice ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.Actor.Invalid");
    }

    [Fact]
    public void ValidatePublication_ActorTooShort_ShouldFailPublishedByTooShort()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = PublishableValidator.ValidatePublication(
            entity,
            NowUtc,
            "x");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.PublishedBy.TooShort");
    }

    [Fact]
    public void ValidatePublication_ValidInput_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = PublishableValidator.ValidatePublication(
            entity,
            NowUtc,
            "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateUnpublication_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange + Act:
        var result = PublishableValidator.ValidateUnpublication<TestPublishable>(
            null!,
            "carol");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.Entity.Required");
    }

    [Fact]
    public void ValidateUnpublication_NotPublished_ShouldFailNotPublished()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = PublishableValidator.ValidateUnpublication(
            entity,
            "carol");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.NotPublished");
    }

    [Fact]
    public void ValidateUnpublication_ActorTooLong_ShouldFailPublishedByTooLong()
    {
        // Arrange:
        var entity = PublishedEntity();
        var actor = new string('a', LifecycleConstant.Constraints.Actor.MaxLength + 1);

        // Act:
        var result = PublishableValidator.ValidateUnpublication(
            entity,
            actor);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.PublishedBy.TooLong");
    }

    [Fact]
    public void ValidateUnpublication_RequireActorWithoutActor_ShouldFailPublishedByRequired()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = PublishableValidator.ValidateUnpublication(
            entity,
            actor: null,
            requireActor: true);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.PublishedBy.Required");
    }

    [Fact]
    public void ValidateUnpublication_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = PublishableValidator.ValidateUnpublication(
            entity,
            "carol ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.Actor.Invalid");
    }

    [Fact]
    public void ValidateUnpublication_ActorTooShort_ShouldFailPublishedByTooShort()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = PublishableValidator.ValidateUnpublication(
            entity,
            "x");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.PublishedBy.TooShort");
    }

    [Fact]
    public void ValidateUnpublication_ValidInput_ShouldSucceed()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = PublishableValidator.ValidateUnpublication(
            entity,
            "carol");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    #endregion

    #region Branch Coverage

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Publish_RequireMatrix_ValidActor_ShouldSucceed(bool requireActor)
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Publish(NowUtc, "alice", requireActor);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.IsPublished.ShouldBeTrue();
        entity.PublishedAtUtc.ShouldBe(NowUtc);
        entity.PublishedBy.ShouldBe("alice");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Unpublish_RequireMatrix_ValidActor_ShouldSucceed(bool requireActor)
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = Success(entity).Unpublish("carol", requireActor);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.IsPublished.ShouldBeFalse();
        entity.PublishedAtUtc.ShouldBeNull();
        entity.PublishedBy.ShouldBeNull();
    }

    [Fact]
    public void Publish_NullActorWithDefaultRequire_ShouldSucceedAndStoreNull()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act: omit requireActor to exercise LifecycleConstant.Defaults.RequireActor path.
        var result = Success(entity).Publish(NowUtc, actor: null);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.IsPublished.ShouldBeTrue();
        entity.PublishedAtUtc.ShouldBe(NowUtc);
        entity.PublishedBy.ShouldBeNull();
    }

    [Fact]
    public void Unpublish_NullActorWithDefaultRequire_ShouldClearAndSucceed()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act: omit requireActor to exercise LifecycleConstant.Defaults.RequireActor path.
        var result = Success(entity).Unpublish(actor: null);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.IsPublished.ShouldBeFalse();
        entity.PublishedAtUtc.ShouldBeNull();
        entity.PublishedBy.ShouldBeNull();
    }

    [Theory]
    [InlineData(false, "Publishable.Actor.Invalid")]
    [InlineData(true, "Publishable.PublishedBy.Required")]
    public void Publish_WhitespaceActor_RequireMatrix_ShouldFailWithExpectedCode(
        bool requireActor,
        string expectedCode)
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Publish(NowUtc, " ", requireActor);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe(expectedCode);
        entity.IsPublished.ShouldBeFalse();
        entity.PublishedAtUtc.ShouldBeNull();
    }

    [Theory]
    [InlineData(false, "Publishable.Actor.Invalid")]
    [InlineData(true, "Publishable.PublishedBy.Required")]
    public void Unpublish_WhitespaceActor_RequireMatrix_ShouldFailWithExpectedCode(
        bool requireActor,
        string expectedCode)
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = Success(entity).Unpublish(" ", requireActor);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe(expectedCode);
        entity.IsPublished.ShouldBeTrue();
        entity.PublishedAtUtc.ShouldBe(NowUtc);
        entity.PublishedBy.ShouldBe("bob");
    }

    [Fact]
    public void Publish_ActorTooShort_ShouldFailPublishedByTooShortWithoutMutation()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Publish(NowUtc, "x");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.PublishedBy.TooShort");
        entity.IsPublished.ShouldBeFalse();
        entity.PublishedAtUtc.ShouldBeNull();
    }

    [Fact]
    public void Unpublish_ActorTooShort_ShouldFailPublishedByTooShortWithoutMutation()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = Success(entity).Unpublish("x");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.PublishedBy.TooShort");
        entity.IsPublished.ShouldBeTrue();
        entity.PublishedAtUtc.ShouldBe(NowUtc);
        entity.PublishedBy.ShouldBe("bob");
    }

    [Fact]
    public void Publish_ActorAtMaxLength_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestPublishable();
        var actor = new string('a', LifecycleConstant.Constraints.Actor.MaxLength);

        // Act:
        var result = Success(entity).Publish(NowUtc, actor);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.PublishedBy.ShouldBe(actor);
    }

    [Fact]
    public void Publish_NegativeOffsetTimestamp_ShouldStoreUtcInstant()
    {
        // Arrange:
        var entity = new TestPublishable();
        var offset = new DateTimeOffset(2024, 5, 1, 7, 0, 0, TimeSpan.FromHours(-5));

        // Act:
        var result = Success(entity).Publish(offset, "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.PublishedAtUtc!.Value.Offset.ShouldBe(TimeSpan.Zero);
        entity.PublishedAtUtc.ShouldBe(offset.ToUniversalTime());
    }

    [Fact]
    public void Publish_AlreadyUtcTimestamp_ShouldKeepZeroOffset()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Publish(NowUtc, "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.PublishedAtUtc!.Value.Offset.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void Publish_NearSkewBoundary_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestPublishable();
        var nearBoundary = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Constraints.Timestamp.MaxFutureSkewSeconds - 5);

        // Act:
        var result = Success(entity).Publish(nearBoundary, "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.IsPublished.ShouldBeTrue();
        entity.PublishedAtUtc.ShouldBe(nearBoundary.ToUniversalTime());
    }

    [Fact]
    public void Publish_JustOverSkewBoundary_ShouldFailInFutureWithoutMutation()
    {
        // Arrange:
        var entity = new TestPublishable();
        var justOver = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Constraints.Timestamp.MaxFutureSkewSeconds + 5);

        // Act:
        var result = Success(entity).Publish(justOver, "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.PublishedAt.InFuture");
        entity.IsPublished.ShouldBeFalse();
        entity.PublishedAtUtc.ShouldBeNull();
    }

    [Fact]
    public void Publish_DoublePublish_ShouldFailSecondAndKeepOriginal()
    {
        // Arrange:
        var entity = new TestPublishable();
        var first = Success(entity).Publish(NowUtc, "alice");
        first.IsSuccess.ShouldBeTrue();

        // Act:
        var result = Success(entity).Publish(NowUtc.AddHours(1), "carol");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.AlreadyPublished");
        entity.IsPublished.ShouldBeTrue();
        entity.PublishedAtUtc.ShouldBe(NowUtc);
        entity.PublishedBy.ShouldBe("alice");
    }

    [Fact]
    public void Unpublish_DoubleUnpublish_ShouldFailSecondAndKeepCleared()
    {
        // Arrange:
        var entity = PublishedEntity();
        var first = Success(entity).Unpublish("carol");
        first.IsSuccess.ShouldBeTrue();

        // Act:
        var result = Success(entity).Unpublish("carol");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Publishable.NotPublished");
        entity.IsPublished.ShouldBeFalse();
        entity.PublishedAtUtc.ShouldBeNull();
        entity.PublishedBy.ShouldBeNull();
    }

    [Fact]
    public void Publish_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestPublishable>.Fail(
            Error.UnprocessableEntity("Test.Fail", "boom"));

        // Act:
        var result = input.Publish(NowUtc, "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Test.Fail");
    }

    [Fact]
    public void Unpublish_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestPublishable>.Fail(
            Error.UnprocessableEntity("Test.Fail", "boom"));

        // Act:
        var result = input.Unpublish("carol");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Test.Fail");
    }

    #endregion

    #region Constants

    [Fact]
    public void LifecycleConstant_ShouldExposeExpectedValues()
    {
        // Assert:
        LifecycleConstant.Constraints.Actor.MinLength.ShouldBe(2);
        LifecycleConstant.Constraints.Actor.MaxLength.ShouldBe(256);
        LifecycleConstant.Constraints.MaxActorLength.ShouldBe(256);
        LifecycleConstant.Constraints.Timestamp.MaxFutureSkewSeconds.ShouldBe(300);
        LifecycleConstant.Defaults.RequireActor.ShouldBeFalse();
        LifecycleConstant.Patterns.Actor.ShouldBe(@"^\S(?:.*\S)?$");
    }

    #endregion

    #endregion
}
