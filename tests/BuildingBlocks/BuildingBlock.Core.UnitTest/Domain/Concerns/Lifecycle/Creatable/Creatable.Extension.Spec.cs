using BuildingBlock.Core.Domain.Concerns.Lifecycle;
using BuildingBlock.Core.Domain.Concerns.Lifecycle.Creatable;
namespace BuildingBlock.Core.Domain.Tests.Concerns.Lifecycle.Creatable;

/// <summary>Specification tests for <see cref="CreatableExtensions"/>.</summary>
public class CreatableExtensionSpec
{
    private static readonly DateTimeOffset NowUtc =
        new(2024, 5, 1, 12, 0, 0, TimeSpan.Zero);

    #region Helper Methods

    /// <summary>Creates a successful result wrapping the given entity.</summary>
    private static Result<ICreatable> Success(ICreatable entity)
    {
        return Result<ICreatable>.Success(entity);
    }

    private sealed class TestCreatable : ICreatable
    {
        public DateTimeOffset CreatedAtUtc { get; set; }
        public string? CreatedBy { get; set; }
    }

    #endregion

    #region Test Cases

    #region InitializeAudit

    [Fact]
    public void InitializeAudit_NewEntity_ShouldSetCreationFields()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.CreatedAtUtc.ShouldBe(NowUtc);
        entity.CreatedBy.ShouldBe("alice");
    }

    [Fact]
    public void InitializeAudit_OffsetTimestamp_ShouldStoreUtcInstant()
    {
        // Arrange:
        var entity = new TestCreatable();
        var offset = new DateTimeOffset(2024, 5, 1, 20, 0, 0, TimeSpan.FromHours(8));

        // Act:
        var result = Success(entity).InitializeAudit(offset, "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.CreatedAtUtc.Offset.ShouldBe(TimeSpan.Zero);
        entity.CreatedAtUtc.ShouldBe(offset.ToUniversalTime());
    }

    [Fact]
    public void InitializeAudit_DefaultNowUtc_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(default, "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Creatable.CreatedAt.Required");
        entity.CreatedAtUtc.ShouldBe(default(DateTimeOffset));
        entity.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void InitializeAudit_RequireCreatedByWithoutActor_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(
            NowUtc,
            actor: null,
            requireCreatedBy: true);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Creatable.CreatedBy.Required");
        entity.CreatedAtUtc.ShouldBe(default(DateTimeOffset));
    }

    [Fact]
    public void InitializeAudit_AlreadyInitialized_ShouldFailAndKeepOriginalValues()
    {
        // Arrange:
        var entity = new TestCreatable
        {
            CreatedAtUtc = NowUtc,
            CreatedBy = "bob"
        };

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Creatable.CreatedAt.AlreadyInitialized");
        entity.CreatedAtUtc.ShouldBe(NowUtc);
        entity.CreatedBy.ShouldBe("bob");
    }

    [Fact]
    public void InitializeAudit_ActorTooLong_ShouldFailCreatedByTooLong()
    {
        // Arrange:
        var entity = new TestCreatable();
        var actor = new string('a', LifecycleConstant.Constraints.MaxActorLength + 1);

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, actor);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Creatable.CreatedBy.TooLong");
        entity.CreatedAtUtc.ShouldBe(default(DateTimeOffset));
    }

    [Fact]
    public void InitializeAudit_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, " alice ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Creatable.Actor.Invalid");
        entity.CreatedAtUtc.ShouldBe(default(DateTimeOffset));
    }

    [Fact]
    public void InitializeAudit_EmptyActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, string.Empty);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Creatable.Actor.Invalid");
    }

    [Fact]
    public void InitializeAudit_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<ICreatable>.Fail(
            Error.UnprocessableEntity("Test.Fail", "boom"));

        // Act:
        var result = input.InitializeAudit(NowUtc, "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Test.Fail");
    }

    #endregion

    #region Validator Specs

    [Fact]
    public void ValidateCreation_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange + Act:
        var result = CreatableValidator.ValidateCreation<ICreatable>(
            null!,
            NowUtc,
            "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Creatable.Entity.Required");
    }

    [Fact]
    public void ValidateCreation_DefaultTimestamp_ShouldFailCreatedAtRequired()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            default,
            "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Creatable.CreatedAt.Required");
    }

    [Fact]
    public void ValidateCreation_FutureTimestampBeyondSkew_ShouldFailCreatedAtInFuture()
    {
        // Arrange:
        var entity = new TestCreatable();
        var future = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Constraints.Timestamp.MaxFutureSkewSeconds + 60);

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            future,
            "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Creatable.CreatedAt.InFuture");
    }

    [Fact]
    public void ValidateCreation_AlreadyInitialized_ShouldFailAlreadyInitialized()
    {
        // Arrange:
        var entity = new TestCreatable
        {
            CreatedAtUtc = NowUtc,
            CreatedBy = "bob"
        };

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            NowUtc,
            "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Creatable.CreatedAt.AlreadyInitialized");
    }

    [Fact]
    public void ValidateCreation_ActorTooLong_ShouldFailCreatedByTooLong()
    {
        // Arrange:
        var entity = new TestCreatable();
        var actor = new string('a', LifecycleConstant.Constraints.Actor.MaxLength + 1);

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            NowUtc,
            actor);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Creatable.CreatedBy.TooLong");
    }

    [Fact]
    public void ValidateCreation_RequireCreatedByWithoutActor_ShouldFailCreatedByRequired()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            NowUtc,
            actor: null,
            requireCreatedBy: true);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Creatable.CreatedBy.Required");
    }

    [Fact]
    public void ValidateCreation_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            NowUtc,
            " alice ");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Creatable.Actor.Invalid");
    }

    [Fact]
    public void ValidateCreation_ActorTooShort_ShouldFailCreatedByTooShort()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            NowUtc,
            "x");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Creatable.CreatedBy.TooShort");
    }

    [Fact]
    public void ValidateCreation_ValidInput_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            NowUtc,
            "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    #endregion

    #region Branch Coverage

    [Fact]
    public void InitializeAudit_NullActorWithDefaultRequire_ShouldSucceedAndStoreNull()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act: omit requireCreatedBy to exercise LifecycleConstant.Defaults.RequireActor path.
        var result = Success(entity).InitializeAudit(NowUtc, actor: null);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.CreatedAtUtc.ShouldBe(NowUtc);
        entity.CreatedBy.ShouldBeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InitializeAudit_RequireMatrix_ValidActor_ShouldSucceed(bool requireCreatedBy)
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, "alice", requireCreatedBy);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.CreatedAtUtc.ShouldBe(NowUtc);
        entity.CreatedBy.ShouldBe("alice");
    }

    [Theory]
    [InlineData(false, "Creatable.Actor.Invalid")]
    [InlineData(true, "Creatable.CreatedBy.Required")]
    public void InitializeAudit_WhitespaceActor_RequireMatrix_ShouldFailWithExpectedCode(
        bool requireCreatedBy,
        string expectedCode)
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, " ", requireCreatedBy);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe(expectedCode);
        entity.CreatedAtUtc.ShouldBe(default(DateTimeOffset));
        entity.CreatedBy.ShouldBeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InitializeAudit_NullActor_RequireMatrix_ShouldMatchExpectation(bool requireCreatedBy)
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, actor: null, requireCreatedBy: requireCreatedBy);

        // Assert:
        if (requireCreatedBy)
        {
            result.IsFailure.ShouldBeTrue();
            result.Errors![0].Code.ShouldBe("Creatable.CreatedBy.Required");
            entity.CreatedAtUtc.ShouldBe(default(DateTimeOffset));
        }
        else
        {
            result.IsSuccess.ShouldBeTrue();
            entity.CreatedAtUtc.ShouldBe(NowUtc);
            entity.CreatedBy.ShouldBeNull();
        }
    }

    [Fact]
    public void InitializeAudit_ActorTooShort_ShouldFailCreatedByTooShortWithoutMutation()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, "x");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Creatable.CreatedBy.TooShort");
        entity.CreatedAtUtc.ShouldBe(default(DateTimeOffset));
        entity.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void InitializeAudit_ActorAtMaxLength_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestCreatable();
        var actor = new string('a', LifecycleConstant.Constraints.Actor.MaxLength);

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, actor);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.CreatedBy.ShouldBe(actor);
    }

    [Fact]
    public void InitializeAudit_NegativeOffsetTimestamp_ShouldStoreUtcInstant()
    {
        // Arrange:
        var entity = new TestCreatable();
        var offset = new DateTimeOffset(2024, 5, 1, 7, 0, 0, TimeSpan.FromHours(-5));

        // Act:
        var result = Success(entity).InitializeAudit(offset, "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.CreatedAtUtc.Offset.ShouldBe(TimeSpan.Zero);
        entity.CreatedAtUtc.ShouldBe(offset.ToUniversalTime());
    }

    [Fact]
    public void InitializeAudit_AlreadyUtcTimestamp_ShouldKeepZeroOffset()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.CreatedAtUtc.Offset.ShouldBe(TimeSpan.Zero);
        entity.CreatedAtUtc.ShouldBe(NowUtc.ToUniversalTime());
    }

    [Fact]
    public void InitializeAudit_NearSkewBoundary_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestCreatable();
        var nearBoundary = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Constraints.Timestamp.MaxFutureSkewSeconds - 5);

        // Act:
        var result = Success(entity).InitializeAudit(nearBoundary, "alice");

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.CreatedAtUtc.ShouldBe(nearBoundary.ToUniversalTime());
    }

    [Fact]
    public void InitializeAudit_JustOverSkewBoundary_ShouldFailCreatedAtInFutureWithoutMutation()
    {
        // Arrange:
        var entity = new TestCreatable();
        var justOver = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Constraints.Timestamp.MaxFutureSkewSeconds + 5);

        // Act:
        var result = Success(entity).InitializeAudit(justOver, "alice");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Creatable.CreatedAt.InFuture");
        entity.CreatedAtUtc.ShouldBe(default(DateTimeOffset));
        entity.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void InitializeAudit_DoubleInitialize_ShouldFailSecondAndKeepOriginal()
    {
        // Arrange:
        var entity = new TestCreatable();
        var first = Success(entity).InitializeAudit(NowUtc, "alice");
        first.IsSuccess.ShouldBeTrue();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc.AddHours(1), "carol");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Creatable.CreatedAt.AlreadyInitialized");
        entity.CreatedAtUtc.ShouldBe(NowUtc);
        entity.CreatedBy.ShouldBe("alice");
    }

    [Fact]
    public void ValidateCreation_NullActorWithDefaultRequire_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            NowUtc,
            actor: null);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
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
