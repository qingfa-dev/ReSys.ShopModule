using BuildingBlock.Core.Domain.Concerns.Lifecycle;
using BuildingBlock.Core.Domain.Concerns.Lifecycle.SoftDeletable;
namespace BuildingBlock.Core.Domain.Tests.Concerns.Lifecycle.SoftDeletable;

/// <summary>Specification tests for <see cref="SoftDeletableExtensions"/>.</summary>
public class SoftDeletableExtensionSpec
{
    private static readonly DateTimeOffset NowUtc =
        new(2024, 5, 1, 12, 0, 0, TimeSpan.Zero);

    #region Helper Methods

    /// <summary>Creates a successful result wrapping the given entity.</summary>
    private static Result<TestSoftDeletable> Success(TestSoftDeletable entity)
    {
        return Result<TestSoftDeletable>.Success(entity);
    }

    private sealed class TestSoftDeletable : ISoftDeletable
    {
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAtUtc { get; set; }
        public string? DeletedBy { get; set; }
    }

    private static TestSoftDeletable DeletedEntity(
        string? deletedBy = "bob",
        DateTimeOffset? deletedAtUtc = null)
    {
        return new TestSoftDeletable
        {
            IsDeleted = true,
            DeletedAtUtc = deletedAtUtc ?? NowUtc,
            DeletedBy = deletedBy
        };
    }

    #endregion

    #region Test Cases

    #region MarkDeleted

    [Fact]
    public void MarkDeleted_ActiveEntity_ShouldSetDeletionFields()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.IsDeleted.ShouldBeTrue();
        entity.DeletedAtUtc.ShouldBe(NowUtc);
        entity.DeletedBy.ShouldBe("alice");
    }

    [Fact]
    public void MarkDeleted_OffsetTimestamp_ShouldStoreUtcInstant()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var offset = new DateTimeOffset(2024, 5, 1, 20, 0, 0, TimeSpan.FromHours(8));

        // Act:
        var result = Success(entity).MarkDeleted(offset, "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.DeletedAtUtc!.Value.Offset.ShouldBe(TimeSpan.Zero);
        entity.DeletedAtUtc.ShouldBe(offset.ToUniversalTime());
    }

    [Fact]
    public void MarkDeleted_DefaultNowUtc_ShouldFailDeletedAtRequiredWithoutMutation()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).MarkDeleted(default, "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.DeletedAt.Required");
        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAtUtc.ShouldBeNull();
    }

    [Fact]
    public void MarkDeleted_AlreadyDeleted_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.AlreadyDeleted");
        entity.DeletedAtUtc.ShouldBe(NowUtc);
        entity.DeletedBy.ShouldBe("bob");
    }

    [Fact]
    public void MarkDeleted_ActorTooLong_ShouldFailDeletedByTooLongWithoutMutation()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var actor = new string('a', LifecycleConstant.Constraints.MaxActorLength + 1);

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, actor);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.DeletedBy.TooLong");
        entity.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void MarkDeleted_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, " alice ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.Actor.Invalid");
        entity.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void MarkDeleted_EmptyActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, string.Empty);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.Actor.Invalid");
    }

    #endregion

    #region Restore

    [Fact]
    public void Restore_DeletedEntity_ShouldClearDeletionFields()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = Success(entity).Restore("carol");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAtUtc.ShouldBeNull();
        entity.DeletedBy.ShouldBeNull();
    }

    [Fact]
    public void Restore_NotDeleted_ShouldFailNotDeletedWithoutMutation()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).Restore("carol");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.NotDeleted");
        entity.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void Restore_RequireActorWithoutActor_ShouldFailDeletedByRequiredWithoutMutation()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = Success(entity).Restore(
            actor: null,
            requireActor: true);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.DeletedBy.Required");
        entity.IsDeleted.ShouldBeTrue();
        entity.DeletedAtUtc.ShouldBe(NowUtc);
    }

    #endregion

    #region Validator Specs

    [Fact]
    public void ValidateDeletion_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange + Act:
        var result = SoftDeletableValidator.ValidateDeletion<TestSoftDeletable>(
            null!,
            NowUtc,
            "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.Entity.Required");
    }

    [Fact]
    public void ValidateDeletion_DefaultTimestamp_ShouldFailDeletedAtRequired()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = SoftDeletableValidator.ValidateDeletion(
            entity,
            default,
            "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.DeletedAt.Required");
    }

    [Fact]
    public void ValidateDeletion_FutureTimestampBeyondSkew_ShouldFailDeletedAtInFuture()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var future = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Constraints.Timestamp.MaxFutureSkewSeconds + 60);

        // Act:
        var result = SoftDeletableValidator.ValidateDeletion(
            entity,
            future,
            "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.DeletedAt.InFuture");
    }

    [Fact]
    public void ValidateDeletion_AlreadyDeleted_ShouldFailAlreadyDeleted()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = SoftDeletableValidator.ValidateDeletion(
            entity,
            NowUtc,
            "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.AlreadyDeleted");
    }

    [Fact]
    public void ValidateDeletion_ActorTooLong_ShouldFailDeletedByTooLong()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var actor = new string('a', LifecycleConstant.Constraints.Actor.MaxLength + 1);

        // Act:
        var result = SoftDeletableValidator.ValidateDeletion(
            entity,
            NowUtc,
            actor);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.DeletedBy.TooLong");
    }

    [Fact]
    public void ValidateDeletion_RequireActorWithoutActor_ShouldFailDeletedByRequired()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = SoftDeletableValidator.ValidateDeletion(
            entity,
            NowUtc,
            actor: null,
            requireActor: true);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.DeletedBy.Required");
    }

    [Fact]
    public void ValidateDeletion_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = SoftDeletableValidator.ValidateDeletion(
            entity,
            NowUtc,
            " alice ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.Actor.Invalid");
    }

    [Fact]
    public void ValidateDeletion_ActorTooShort_ShouldFailDeletedByTooShort()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = SoftDeletableValidator.ValidateDeletion(
            entity,
            NowUtc,
            "x");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.DeletedBy.TooShort");
    }

    [Fact]
    public void ValidateDeletion_ValidInput_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = SoftDeletableValidator.ValidateDeletion(
            entity,
            NowUtc,
            "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateRestoration_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange + Act:
        var result = SoftDeletableValidator.ValidateRestoration<TestSoftDeletable>(
            null!,
            "carol");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.Entity.Required");
    }

    [Fact]
    public void ValidateRestoration_NotDeleted_ShouldFailNotDeleted()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = SoftDeletableValidator.ValidateRestoration(
            entity,
            "carol");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.NotDeleted");
    }

    [Fact]
    public void ValidateRestoration_ActorTooLong_ShouldFailDeletedByTooLong()
    {
        // Arrange:
        var entity = DeletedEntity();
        var actor = new string('a', LifecycleConstant.Constraints.Actor.MaxLength + 1);

        // Act:
        var result = SoftDeletableValidator.ValidateRestoration(
            entity,
            actor);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.DeletedBy.TooLong");
    }

    [Fact]
    public void ValidateRestoration_RequireActorWithoutActor_ShouldFailDeletedByRequired()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = SoftDeletableValidator.ValidateRestoration(
            entity,
            actor: null,
            requireActor: true);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.DeletedBy.Required");
    }

    [Fact]
    public void ValidateRestoration_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = SoftDeletableValidator.ValidateRestoration(
            entity,
            "carol ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.Actor.Invalid");
    }

    [Fact]
    public void ValidateRestoration_ActorTooShort_ShouldFailDeletedByTooShort()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = SoftDeletableValidator.ValidateRestoration(
            entity,
            "x");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.DeletedBy.TooShort");
    }

    [Fact]
    public void ValidateRestoration_ValidInput_ShouldSucceed()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = SoftDeletableValidator.ValidateRestoration(
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
    public void MarkDeleted_RequireMatrix_ValidActor_ShouldSucceed(bool requireActor)
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, "alice", requireActor);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.IsDeleted.ShouldBeTrue();
        entity.DeletedAtUtc.ShouldBe(NowUtc);
        entity.DeletedBy.ShouldBe("alice");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Restore_RequireMatrix_ValidActor_ShouldSucceed(bool requireActor)
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = Success(entity).Restore("carol", requireActor);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAtUtc.ShouldBeNull();
        entity.DeletedBy.ShouldBeNull();
    }

    [Fact]
    public void MarkDeleted_NullActorWithDefaultRequire_ShouldSucceedAndStoreNull()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act: omit requireActor to exercise LifecycleConstant.Defaults.RequireActor path.
        var result = Success(entity).MarkDeleted(NowUtc, actor: null);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.IsDeleted.ShouldBeTrue();
        entity.DeletedAtUtc.ShouldBe(NowUtc);
        entity.DeletedBy.ShouldBeNull();
    }

    [Fact]
    public void Restore_NullActorWithDefaultRequire_ShouldClearAndSucceed()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act: omit requireActor to exercise LifecycleConstant.Defaults.RequireActor path.
        var result = Success(entity).Restore(actor: null);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAtUtc.ShouldBeNull();
        entity.DeletedBy.ShouldBeNull();
    }

    [Theory]
    [InlineData(false, "SoftDeletable.Actor.Invalid")]
    [InlineData(true, "SoftDeletable.DeletedBy.Required")]
    public void MarkDeleted_WhitespaceActor_RequireMatrix_ShouldFailWithExpectedCode(
        bool requireActor,
        string expectedCode)
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, " ", requireActor);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe(expectedCode);
        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAtUtc.ShouldBeNull();
    }

    [Theory]
    [InlineData(false, "SoftDeletable.Actor.Invalid")]
    [InlineData(true, "SoftDeletable.DeletedBy.Required")]
    public void Restore_WhitespaceActor_RequireMatrix_ShouldFailWithExpectedCode(
        bool requireActor,
        string expectedCode)
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = Success(entity).Restore(" ", requireActor);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe(expectedCode);
        entity.IsDeleted.ShouldBeTrue();
        entity.DeletedAtUtc.ShouldBe(NowUtc);
        entity.DeletedBy.ShouldBe("bob");
    }

    [Fact]
    public void MarkDeleted_ActorTooShort_ShouldFailDeletedByTooShortWithoutMutation()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, "x");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.DeletedBy.TooShort");
        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAtUtc.ShouldBeNull();
    }

    [Fact]
    public void Restore_ActorTooShort_ShouldFailDeletedByTooShortWithoutMutation()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = Success(entity).Restore("x");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.DeletedBy.TooShort");
        entity.IsDeleted.ShouldBeTrue();
        entity.DeletedAtUtc.ShouldBe(NowUtc);
        entity.DeletedBy.ShouldBe("bob");
    }

    [Fact]
    public void MarkDeleted_ActorAtMaxLength_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var actor = new string('a', LifecycleConstant.Constraints.Actor.MaxLength);

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, actor);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.DeletedBy.ShouldBe(actor);
    }

    [Fact]
    public void MarkDeleted_NegativeOffsetTimestamp_ShouldStoreUtcInstant()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var offset = new DateTimeOffset(2024, 5, 1, 7, 0, 0, TimeSpan.FromHours(-5));

        // Act:
        var result = Success(entity).MarkDeleted(offset, "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.DeletedAtUtc!.Value.Offset.ShouldBe(TimeSpan.Zero);
        entity.DeletedAtUtc.ShouldBe(offset.ToUniversalTime());
    }

    [Fact]
    public void MarkDeleted_AlreadyUtcTimestamp_ShouldKeepZeroOffset()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.DeletedAtUtc!.Value.Offset.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void MarkDeleted_NearSkewBoundary_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var nearBoundary = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Constraints.Timestamp.MaxFutureSkewSeconds - 5);

        // Act:
        var result = Success(entity).MarkDeleted(nearBoundary, "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.IsDeleted.ShouldBeTrue();
        entity.DeletedAtUtc.ShouldBe(nearBoundary.ToUniversalTime());
    }

    [Fact]
    public void MarkDeleted_JustOverSkewBoundary_ShouldFailInFutureWithoutMutation()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var justOver = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Constraints.Timestamp.MaxFutureSkewSeconds + 5);

        // Act:
        var result = Success(entity).MarkDeleted(justOver, "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.DeletedAt.InFuture");
        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAtUtc.ShouldBeNull();
    }

    [Fact]
    public void MarkDeleted_DoubleDelete_ShouldFailSecondAndKeepOriginal()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var first = Success(entity).MarkDeleted(NowUtc, "alice");
        first.IsSuccess.ShouldBeTrue();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc.AddHours(1), "carol");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.AlreadyDeleted");
        entity.IsDeleted.ShouldBeTrue();
        entity.DeletedAtUtc.ShouldBe(NowUtc);
        entity.DeletedBy.ShouldBe("alice");
    }

    [Fact]
    public void Restore_DoubleRestore_ShouldFailSecondAndKeepCleared()
    {
        // Arrange:
        var entity = DeletedEntity();
        var first = Success(entity).Restore("carol");
        first.IsSuccess.ShouldBeTrue();

        // Act:
        var result = Success(entity).Restore("carol");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("SoftDeletable.NotDeleted");
        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAtUtc.ShouldBeNull();
        entity.DeletedBy.ShouldBeNull();
    }

    [Fact]
    public void MarkDeleted_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestSoftDeletable>.Fail(
            Error.UnprocessableEntity("Test.Fail", "boom"));

        // Act:
        var result = input.MarkDeleted(NowUtc, "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Test.Fail");
    }

    [Fact]
    public void Restore_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestSoftDeletable>.Fail(
            Error.UnprocessableEntity("Test.Fail", "boom"));

        // Act:
        var result = input.Restore("carol");

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
