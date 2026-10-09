using BuildingBlock.Monad.Errors;
using BuildingBlock.Monad.Metadata;
using BuildingBlock.Monad.Results;

namespace BuildingBlock.Monad.UnitTest.Results;

/// <summary>Tests for <see cref="ResultExtension"/> metadata and status extensions.</summary>
[Trait("Category", "Unit")]
public class ResultExtensionSpec
{
    #region Metadata and Status

    /// <summary>Non-generic metadata and status extensions should return copies.</summary>
    [Theory]
    [InlineData("original", "updated")]
    [InlineData("first", "second")]
    public void NonGeneric_Metadata_And_Status_Extensions_Should_Return_Copies(
        string originalValue,
        string updatedValue)
    {
        // Arrange
        var original = Result.Ok()
            .WithMetadata("source", originalValue);

        // Act
        var copy = original
            .WithMetadata("source", updatedValue)
            .WithStatus(ResultConstant.StatusCode.Accepted);

        // Assert
        copy.ShouldNotBeSameAs(original);
        copy.Metadata["source"].ShouldBe(updatedValue);
        copy.StatusCode.ShouldBe(ResultConstant.StatusCode.Accepted);
        original.Metadata["source"].ShouldBe(originalValue);
        original.StatusCode.ShouldBe(ResultConstant.StatusCode.Ok);
    }

    /// <summary>Generic metadata and status extensions should return copies.</summary>
    [Theory]
    [InlineData(5)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Generic_Metadata_And_Status_Extensions_Should_Return_Copies(int value)
    {
        // Arrange
        var original = Result<int>.Ok(value)
            .WithMetadata("source", "original");

        // Act
        var copy = original
            .WithMetadata("source", "updated")
            .WithStatus(ResultConstant.StatusCode.Accepted);

        // Assert
        copy.ShouldNotBeSameAs(original);
        copy.Metadata["source"].ShouldBe("updated");
        copy.StatusCode.ShouldBe(ResultConstant.StatusCode.Accepted);
        copy.Value.ShouldBe(value);
        original.Metadata["source"].ShouldBe("original");
        original.StatusCode.ShouldBe(ResultConstant.StatusCode.Ok);
    }

    #endregion

    #region Metadata Dictionary

    /// <summary>Dictionary metadata overloads should merge into copies.</summary>
    [Fact]
    public void MetadataDictionary_Overloads_Should_Merge_Into_Copies()
    {
        // Arrange
        var seed = new MetadataDictionary { ["seed"] = "s" };
        var genericSeed = new MetadataDictionary { ["seed"] = "s" };

        // Act
        var nonGeneric = Result.Ok().WithMetadata(seed);
        var generic = Result<int>.Ok(5).WithMetadata(genericSeed);

        // Assert
        nonGeneric.Metadata["seed"].ShouldBe("s");
        generic.Metadata["seed"].ShouldBe("s");
        generic.Value.ShouldBe(5);
        seed.Keys.ShouldBe(["seed"]);
        genericSeed.Keys.ShouldBe(["seed"]);
    }

    #endregion

    #region Standard Metadata

    /// <summary>Standard metadata helpers should set their keys on both shapes.</summary>
    [Fact]
    public void Standard_Metadata_Helpers_Should_Set_Their_Keys()
    {
        // Arrange
        var nonGeneric = Result.Ok();
        var generic = Result<int>.Ok(7);

        // Act
        var stamped = nonGeneric
            .WithTraceId("trace-1")
            .WithTimestamp("ts-1")
            .WithResource("resource-1")
            .WithField("field-1")
            .WithAttempt(1);
        var genericStamped = generic
            .WithTraceId("trace-2")
            .WithTimestamp("ts-2")
            .WithResource("resource-2")
            .WithField("field-2")
            .WithAttempt(2);

        // Assert
        stamped.Metadata[ErrorConstant.Metadata.TraceId].ShouldBe("trace-1");
        stamped.Metadata[ErrorConstant.Metadata.Timestamp].ShouldBe("ts-1");
        stamped.Metadata[ErrorConstant.Metadata.Resource].ShouldBe("resource-1");
        stamped.Metadata[ErrorConstant.Metadata.Field].ShouldBe("field-1");
        stamped.Metadata[ErrorConstant.Metadata.Attempt].ShouldBe(1);
        stamped.IsSuccess.ShouldBeTrue();

        genericStamped.Metadata[ErrorConstant.Metadata.TraceId].ShouldBe("trace-2");
        genericStamped.Metadata[ErrorConstant.Metadata.Timestamp].ShouldBe("ts-2");
        genericStamped.Metadata[ErrorConstant.Metadata.Resource].ShouldBe("resource-2");
        genericStamped.Metadata[ErrorConstant.Metadata.Field].ShouldBe("field-2");
        genericStamped.Metadata[ErrorConstant.Metadata.Attempt].ShouldBe(2);
        genericStamped.Value.ShouldBe(7);
    }

    #endregion
}
