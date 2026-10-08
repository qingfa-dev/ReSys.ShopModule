using BuildingBlock.Monad.Metadata;

namespace BuildingBlock.Monad.UnitTest.Metadata;

/// <summary>
/// Tests for <see cref="HasMetadataExtensions"/> metadata access,
/// well-known metadata keys, and argument guards.
/// </summary>
/// <remarks>
/// Covers metadata delegation, well-known metadata accessors,
/// missing values, typed conversion, and null argument guards.
/// </remarks>
[Trait("Category", "Unit")]
public class HasMetadataExtensionSpec
{
    #region Helpers

    /// <summary>
    /// Creates a metadata-bearing stub containing one key/value pair.
    /// </summary>
    private static HasMetadataStub<MetadataDictionary> StubWith(
        string key,
        object value)
        => new()
        {
            Metadata = MetadataDictionary.Create()
                .With(key, value)
        };

    /// <summary>
    /// Creates a metadata-bearing stub with empty metadata.
    /// </summary>
    private static HasMetadataStub<MetadataDictionary> EmptyStub()
        => new()
        {
            Metadata = MetadataDictionary.Create()
        };

    #endregion

    #region GetValueOrDefault

    [Fact]
    public void GetValueOrDefault_When_Key_Present_Should_Return_Raw_Value()
    {
        // Arrange
        IHasMetadata<MetadataDictionary> source =
            StubWith("k", 42);

        // Act
        var actual = source.GetValueOrDefault("k");

        // Assert
        actual.ShouldBe(42);
    }

    [Fact]
    public void GetValueOrDefault_When_Key_Missing_Should_Return_Null()
    {
        // Arrange
        IHasMetadata<MetadataDictionary> source =
            EmptyStub();

        // Act
        var actual = source.GetValueOrDefault("k");

        // Assert
        actual.ShouldBeNull();
    }

     [Fact]
     public void GetValueOrDefault_When_Key_Present_Should_Return_Converted_Value()
     {
         // Arrange
         IHasMetadata<MetadataDictionary> source =
             StubWith("k", "42");

         // Act
         var actual =
             source.GetValueOrDefault<MetadataDictionary, int>("k");

         // Assert
         actual.ShouldBe(42);
     }

    [Fact]
    public void GetValueOrDefault_With_Fallback_When_Key_Missing_Should_Return_Fallback()
    {
        // Arrange
        IHasMetadata<MetadataDictionary> source =
            EmptyStub();

        // Act
        var actual =
            source.GetValueOrDefault("version", 7);

        // Assert
        actual.ShouldBe(7);
    }

    [Fact]
    public void GetValueOrDefault_When_Source_Is_Null_Should_Throw()
    {
        // Arrange
        IHasMetadata<MetadataDictionary> source = null!;

        // Act
        Action act =
            () => source.GetValueOrDefault("k");

        // Assert
        var exception =
            Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("source");
    }

    #endregion

    #region GetRequiredValue

    [Fact]
    public void GetRequiredValue_When_Key_Present_Should_Return_Raw_Value()
    {
        // Arrange
        IHasMetadata<MetadataDictionary> source =
            StubWith("k", 42);

        // Act
        var actual =
            source.GetRequiredValue("k");

        // Assert
        actual.ShouldBe(42);
    }

    [Fact]
    public void GetRequiredValue_When_Key_Missing_Should_Throw()
    {
        // Arrange
        IHasMetadata<MetadataDictionary> source =
            EmptyStub();

        // Act
        Action act =
            () => source.GetRequiredValue("k");

        // Assert
        Should.Throw<KeyNotFoundException>(act);
    }

     [Fact]
     public void GetRequiredValue_When_Value_Is_Convertible_Should_Return_Converted_Value()
     {
         // Arrange
         IHasMetadata<MetadataDictionary> source =
             StubWith("k", "42");

         // Act
         var actual =
             source.GetRequiredValue<MetadataDictionary, int>("k");

         // Assert
         actual.ShouldBe(42);
     }

     [Fact]
     public void GetRequiredValue_When_Value_Is_Not_Convertible_Should_Throw()
     {
         // Arrange
         IHasMetadata<MetadataDictionary> source =
             StubWith("k", "abc");

         // Act
         Action act =
             () => source.GetRequiredValue<MetadataDictionary, int>("k");

         // Assert
         Should.Throw<InvalidCastException>(act);
     }

    #endregion

    #region TryGetValue

    [Fact]
    public void TryGetValue_When_Key_Present_And_Convertible_Should_Return_True()
    {
        // Arrange
        IHasMetadata<MetadataDictionary> source =
            StubWith("k", "42");

        // Act
        var found =
            source.TryGetValue("k", out int value);

        // Assert
        found.ShouldBeTrue();
        value.ShouldBe(42);
    }

    [Fact]
    public void TryGetValue_When_Key_Missing_Should_Return_False()
    {
        // Arrange
        IHasMetadata<MetadataDictionary> source =
            EmptyStub();

        // Act
        var found =
            source.TryGetValue("k", out int value);

        // Assert
        found.ShouldBeFalse();
        value.ShouldBe(0);
    }

    [Fact]
    public void TryGetValue_When_Value_Is_Incompatible_Should_Return_False()
    {
        // Arrange
        IHasMetadata<MetadataDictionary> source =
            StubWith("k", "abc");

        // Act
        var found =
            source.TryGetValue("k", out int value);

        // Assert
        found.ShouldBeFalse();
        value.ShouldBe(0);
    }

    [Fact]
    public void TryGetValue_When_Source_Is_Null_Should_Throw()
    {
        // Arrange
        IHasMetadata<MetadataDictionary> source = null!;

        // Act
        Action act =
            () => source.TryGetValue("k", out int _);

        // Assert
        Should.Throw<ArgumentNullException>(act);
    }

    #endregion

    #region Contains

    [Fact]
    public void Contains_When_Key_Is_Present_Should_Return_True()
    {
        // Arrange
        IHasMetadata<MetadataDictionary> source =
            StubWith("k", 42);

        // Act
        var actual =
            source.Contains("k");

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public void Contains_When_Key_Is_Missing_Should_Return_False()
    {
        // Arrange
        IHasMetadata<MetadataDictionary> source =
            EmptyStub();

        // Act
        var actual =
            source.Contains("k");

        // Assert
        actual.ShouldBeFalse();
    }

    [Fact]
    public void Contains_When_Source_Is_Null_Should_Throw()
    {
        // Arrange
        IHasMetadata<MetadataDictionary> source = null!;

        // Act
        Action act =
            () => source.Contains("k");

        // Assert
        Should.Throw<ArgumentNullException>(act);
    }

    #endregion

    #region SetValue

    [Fact]
    public void SetValue_Should_Update_Underlying_Metadata()
    {
        // Arrange
        var source =
            EmptyStub();

        // Act
        source.SetValue("k", 42);

        // Assert
        source.Metadata["k"].ShouldBe(42);
    }

    [Fact]
    public void SetValue_Should_Overwrite_Existing_Value()
    {
        // Arrange
        var source =
            StubWith("k", 1);

        // Act
        source.SetValue("k", 2);

        // Assert
        source.Metadata["k"].ShouldBe(2);
    }

    [Fact]
    public void SetValue_When_Source_Is_Null_Should_Throw()
    {
        // Arrange
        IHasMetadata<MetadataDictionary> source = null!;

        // Act
        Action act =
            () => source.SetValue("k", 42);

        // Assert
        Should.Throw<ArgumentNullException>(act);
    }

    #endregion

    #region Remove

    [Fact]
    public void Remove_When_Key_Is_Present_Should_Remove_And_Return_True()
    {
        // Arrange
        var source =
            StubWith("k", 42);

        // Act
        var removed =
            source.Remove("k");

        // Assert
        removed.ShouldBeTrue();
        source.Metadata.ContainsKey("k").ShouldBeFalse();
    }

    [Fact]
    public void Remove_When_Key_Is_Missing_Should_Return_False()
    {
        // Arrange
        var source =
            EmptyStub();

        // Act
        var removed =
            source.Remove("k");

        // Assert
        removed.ShouldBeFalse();
    }

    [Fact]
    public void Remove_When_Source_Is_Null_Should_Throw()
    {
        // Arrange
        IHasMetadata<MetadataDictionary> source = null!;

        // Act
        Action act =
            () => source.Remove("k");

        // Assert
        Should.Throw<ArgumentNullException>(act);
    }

    #endregion

    #region Well-known Metadata

     [Fact]
     public void CorrelationId_Should_Return_Metadata_Value()
     {
         // Arrange
         var expected =
             Guid.Parse("11111111-2222-3333-4444-555555555555");

         IHasMetadata<MetadataDictionary> source =
             StubWith(
                 MetadataConstant.Keyword.CorrelationId,
                 expected.ToString());

         // Act
         var actual =
             source.GetCorrelationId();

         // Assert
         actual.ShouldBe(expected);
     }

     [Fact]
     public void RequestId_Should_Return_Metadata_Value()
     {
         // Arrange
         var expected =
             Guid.Parse("22222222-3333-4444-5555-666666666666");

         IHasMetadata<MetadataDictionary> source =
             StubWith(
                 MetadataConstant.Keyword.RequestId,
                 expected.ToString());

         // Act
         var actual =
             source.GetRequestId();

         // Assert
         actual.ShouldBe(expected);
     }

     [Fact]
     public void CausationId_Should_Return_Metadata_Value()
     {
         // Arrange
         var expected =
             Guid.Parse("33333333-4444-5555-6666-777777777777");

         IHasMetadata<MetadataDictionary> source =
             StubWith(
                 MetadataConstant.Keyword.CausationId,
                 expected.ToString());

         // Act
         var actual =
             source.GetCausationId();

         // Assert
         actual.ShouldBe(expected);
     }

    [Fact]
    public void Timestamp_Should_Return_Metadata_Value()
    {
        // Arrange
        var expected =
            DateTimeOffset.UtcNow;

        IHasMetadata<MetadataDictionary> source =
            StubWith(
                MetadataConstant.Keyword.Timestamp,
                expected);

        // Act
        var actual =
            source.GetTimestamp();

        // Assert
        actual.ShouldBe(expected);
    }

    #endregion

    #region Contract

    [Fact]
    public void Extension_Should_Accept_Generic_Metadata_Contract()
    {
        // Arrange
        IHasMetadata<MetadataDictionary> source =
            EmptyStub();

        // Act
        var actual =
            source.GetValueOrDefault("missing");

        // Assert
        actual.ShouldBeNull();
    }

    #endregion
}
