using BuildingBlock.Core.Domain.Concerns.Organization.Hierarchical;
namespace BuildingBlock.Core.Domain.Tests.Concerns.Organization.Hierarchical;

/// <summary>Specification for <see cref="HierarchicalExtensions"/>.</summary>
public class HierarchicalExtensionSpec
{
    #region Helper Methods

    private sealed class TestHierarchical : IHierarchical<Guid>
    {
        public Guid? ParentId { get; set; }
    }

    /// <summary>Creates a successful result for the test entity.</summary>
    private static Result<TestHierarchical> Success(
        TestHierarchical entity)
    {
        return Result<TestHierarchical>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void SetParent_WithParentId_ShouldSetParentId()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var parentId = Guid.NewGuid();

        // Act:
        var result = Success(entity).SetParent(parentId);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.ParentId.ShouldBe(parentId);
    }

    [Fact]
    public void SetParent_Null_ShouldClearParentId()
    {
        // Arrange:
        var entity = new TestHierarchical
        {
            ParentId = Guid.NewGuid()
        };

        // Act:
        var result = Success(entity)
            .SetParent<TestHierarchical, Guid>(null);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.ParentId.ShouldBeNull();
    }

    [Fact]
    public void SetParent_NewParent_ShouldReplaceParentId()
    {
        // Arrange:
        var entity = new TestHierarchical
        {
            ParentId = Guid.NewGuid()
        };
        var parentId = Guid.NewGuid();

        // Act:
        var result = Success(entity).SetParent(parentId);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.ParentId.ShouldBe(parentId);
    }

    [Fact]
    public void ValidateParent_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            (TestHierarchical)null!,
            Guid.NewGuid());

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Hierarchical.Entity.Required");
    }

    #endregion

    #region Validator Specs

    [Fact]
    public void ValidateParent_EmptyGuid_ShouldFailParentEmpty()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            entity,
            Guid.Empty);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Hierarchical.Parent.Empty");
    }

    [Fact]
    public void ValidateParent_NullParent_ShouldSucceedAsRoot()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            entity,
            (Guid?)null);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateParent_ValidParent_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            entity,
            Guid.NewGuid());

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateParent_SelfParent_ShouldFailSelfParent()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var id = Guid.NewGuid();

        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            entity,
            id,
            id);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Hierarchical.Parent.SelfParent");
    }

    [Fact]
    public void ValidateParent_DifferentEntityId_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            entity,
            Guid.NewGuid(),
            Guid.NewGuid());

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateParent_ParentInAncestors_ShouldFailCycleDetected()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var entityId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var ancestors = new[] { parentId, Guid.NewGuid() };

        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            entity,
            parentId,
            entityId,
            ancestors);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Hierarchical.Parent.CycleDetected");
    }

    [Fact]
    public void ValidateParent_EntityInAncestors_ShouldFailCycleDetected()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var entityId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var ancestors = new[] { entityId, Guid.NewGuid() };

        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            entity,
            parentId,
            entityId,
            ancestors);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Hierarchical.Parent.CycleDetected");
    }

    [Fact]
    public void ValidateParent_NoCycle_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            entity,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new[] { Guid.NewGuid() });

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateDepth_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = HierarchicalValidator.ValidateDepth(
            (TestHierarchical)null!,
            0);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Hierarchical.Entity.Required");
    }

    [Fact]
    public void ValidateDepth_Negative_ShouldFailDepthExceeded()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = HierarchicalValidator.ValidateDepth(
            entity,
            HierarchicalConstant.Constraints.Level.Min - 1);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Hierarchical.Depth.Exceeded");
    }

    [Fact]
    public void ValidateDepth_TooDeep_ShouldFailDepthExceeded()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = HierarchicalValidator.ValidateDepth(
            entity,
            HierarchicalConstant.Constraints.Parent.MaxDepth + 1);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Hierarchical.Depth.Exceeded");
    }

    [Fact]
    public void ValidateDepth_Valid_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var root = HierarchicalValidator.ValidateDepth(
            entity,
            HierarchicalConstant.Constraints.Level.Min);
        var max = HierarchicalValidator.ValidateDepth(
            entity,
            HierarchicalConstant.Constraints.Parent.MaxDepth);

        // Assert:
        root.IsSuccess.ShouldBeTrue();
        max.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidatePath_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = HierarchicalValidator.ValidatePath(
            (TestHierarchical)null!,
            "a/b");

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Hierarchical.Entity.Required");
    }

    [Fact]
    public void ValidatePath_TooLong_ShouldFailPathTooLong()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var longPath = new string(
            'a',
            HierarchicalConstant.Constraints.Parent.MaxPathLength + 1);

        // Act:
        var result = HierarchicalValidator.ValidatePath(
            entity,
            longPath);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Hierarchical.Path.TooLong");
    }

    [Fact]
    public void ValidatePath_Valid_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var nullPath = HierarchicalValidator.ValidatePath(
            entity,
            null);
        var normalPath = HierarchicalValidator.ValidatePath(
            entity,
            "root/child");

        // Assert:
        nullPath.IsSuccess.ShouldBeTrue();
        normalPath.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Move_ValidParent_ShouldSetParentId()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var parentId = Guid.NewGuid();

        // Act:
        var result = Success(entity).Move<TestHierarchical, Guid>(
            parentId,
            Guid.NewGuid());

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.ParentId.ShouldBe(parentId);
    }

    [Fact]
    public void Move_SelfParent_ShouldFailWithoutMutation()
    {
        // Arrange:
        var id = Guid.NewGuid();
        var entity = new TestHierarchical();

        // Act:
        var result = Success(entity).Move<TestHierarchical, Guid>(
            id,
            id);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Hierarchical.Parent.SelfParent");
        entity.ParentId.ShouldBeNull();
    }

    [Fact]
    public void Reparent_Cycle_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var parentId = Guid.NewGuid();
        var ancestors = new[] { parentId };

        // Act:
        var result = Success(entity).Reparent<TestHierarchical, Guid>(
            parentId,
            Guid.NewGuid(),
            ancestors);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Hierarchical.Parent.CycleDetected");
        entity.ParentId.ShouldBeNull();
    }

    [Fact]
    public void Reparent_Valid_ShouldSetParentId()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var parentId = Guid.NewGuid();

        // Act:
        var result = Success(entity).Reparent<TestHierarchical, Guid>(
            parentId,
            Guid.NewGuid(),
            new[] { Guid.NewGuid() });

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.ParentId.ShouldBe(parentId);
    }

    [Fact]
    public void ClearParent_AssignedParent_ShouldClearParentId()
    {
        // Arrange:
        var entity = new TestHierarchical
        {
            ParentId = Guid.NewGuid()
        };

        // Act:
        var result = Success(entity).ClearParent<TestHierarchical, Guid>();

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.ParentId.ShouldBeNull();
    }

    [Fact]
    public void SetParent_EmptyGuid_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestHierarchical
        {
            ParentId = Guid.NewGuid()
        };
        var original = entity.ParentId;

        // Act:
        var result = Success(entity)
            .SetParent<TestHierarchical, Guid>(Guid.Empty);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Hierarchical.Parent.Empty");
        entity.ParentId.ShouldBe(original);
    }

    [Fact]
    public void SetParent_NonNullableEmptyGuid_ShouldFailParentEmpty()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = Success(entity).SetParent<TestHierarchical, Guid>(Guid.Empty);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Hierarchical.Parent.Empty");
        entity.ParentId.ShouldBeNull();
    }

    [Fact]
    public void Move_NullParent_ShouldMakeRoot()
    {
        // Arrange:
        var entity = new TestHierarchical
        {
            ParentId = Guid.NewGuid()
        };

        // Act:
        var result = Success(entity).Move<TestHierarchical, Guid>(
            (Guid?)null,
            Guid.NewGuid());

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.ParentId.ShouldBeNull();
    }

    [Fact]
    public void Move_NullEntityId_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var parentId = Guid.NewGuid();

        // Act:
        var result = Success(entity).Move<TestHierarchical, Guid>(
            parentId,
            (Guid?)null);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.ParentId.ShouldBe(parentId);
    }

    [Fact]
    public void Move_EmptyParent_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var entityId = Guid.NewGuid();

        // Act:
        var result = Success(entity).Move<TestHierarchical, Guid>(
            Guid.Empty,
            entityId);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Hierarchical.Parent.Empty");
        entity.ParentId.ShouldBeNull();
    }

    [Fact]
    public void Reparent_NullParent_ShouldMakeRoot()
    {
        // Arrange:
        var entity = new TestHierarchical
        {
            ParentId = Guid.NewGuid()
        };

        // Act:
        var result = Success(entity).Reparent<TestHierarchical, Guid>(
            (Guid?)null,
            Guid.NewGuid(),
            new[] { Guid.NewGuid() });

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.ParentId.ShouldBeNull();
    }

    [Fact]
    public void Reparent_SelfParent_ShouldFailWithoutMutation()
    {
        // Arrange:
        var id = Guid.NewGuid();
        var entity = new TestHierarchical();

        // Act:
        var result = Success(entity).Reparent<TestHierarchical, Guid>(
            id,
            id,
            Array.Empty<Guid>());

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Hierarchical.Parent.SelfParent");
        entity.ParentId.ShouldBeNull();
    }

    [Fact]
    public void Reparent_EntityIdInAncestors_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var entityId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var ancestors = new[] { entityId, Guid.NewGuid() };

        // Act:
        var result = Success(entity).Reparent<TestHierarchical, Guid>(
            parentId,
            entityId,
            ancestors);

        // Assert:
        result.IsFailure.ShouldBeTrue();
        result.Errors![0].Code.ShouldBe("Hierarchical.Parent.CycleDetected");
        entity.ParentId.ShouldBeNull();
    }

    [Fact]
    public void ClearParent_AlreadyRoot_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = Success(entity).ClearParent<TestHierarchical, Guid>();

        // Assert:
        result.IsSuccess.ShouldBeTrue();
        entity.ParentId.ShouldBeNull();
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(5, true)]
    [InlineData(32, true)]
    [InlineData(33, false)]
    public void ValidateDepth_Boundaries_ShouldMatchRange(int depth, bool expectedSuccess)
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = HierarchicalValidator.ValidateDepth(entity, depth);

        // Assert:
        if (expectedSuccess)
        {
            result.IsSuccess.ShouldBeTrue();
        }
        else
        {
            result.IsFailure.ShouldBeTrue();
            result.Errors![0].Code.ShouldBe("Hierarchical.Depth.Exceeded");
        }
    }

    [Fact]
    public void ValidatePath_AtLimit_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var atLimit = new string(
            'a',
            HierarchicalConstant.Constraints.Parent.MaxPathLength);

        // Act:
        var result = HierarchicalValidator.ValidatePath(entity, atLimit);

        // Assert:
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void FailureCodes_ShouldHaveExpectedCodes()
    {
        // Arrange:
        // Act:
        // Assert:
        HierarchicalResult.Failure.EntityRequired.Code.ShouldBe("Hierarchical.Entity.Required");
        HierarchicalResult.Failure.ParentEmpty.Code.ShouldBe("Hierarchical.Parent.Empty");
        HierarchicalResult.Failure.SelfParent.Code.ShouldBe("Hierarchical.Parent.SelfParent");
        HierarchicalResult.Failure.CycleDetected.Code.ShouldBe("Hierarchical.Parent.CycleDetected");
        HierarchicalResult.Failure.DepthExceeded.Code.ShouldBe("Hierarchical.Depth.Exceeded");
        HierarchicalResult.Failure.LevelOutOfRange.Code.ShouldBe("Hierarchical.Level.OutOfRange");
        HierarchicalResult.Failure.PathTooLong.Code.ShouldBe("Hierarchical.Path.TooLong");
    }

    #endregion

    #region Constant Specs

    [Fact]
    public void Constants_ShouldMatchNestedConstraints()
    {
        // Arrange:
        // Act:
        // Assert:
        HierarchicalConstant.Constraints.Parent.MaxDepth.ShouldBe(32);
        HierarchicalConstant.Constraints.Parent.MaxPathLength.ShouldBe(1024);
        HierarchicalConstant.Constraints.Level.Min.ShouldBe(0);
        HierarchicalConstant.Constraints.Level.Max.ShouldBe(32);
        HierarchicalConstant.Defaults.RootLevel.ShouldBe(0);
        HierarchicalConstant.Defaults.PathSeparator.ShouldBe('/');
    }

    #endregion
}
