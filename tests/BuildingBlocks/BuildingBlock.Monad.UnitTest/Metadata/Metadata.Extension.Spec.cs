using System.Collections.ObjectModel;

using BuildingBlock.Monad.Metadata;

namespace BuildingBlock.Monad.UnitTest.Metadata;

/// <summary>
/// Tests for <see cref="MetadataExtensions"/> typed access, conversion,
/// presence, and mutation.
/// </summary>
/// <remarks>
/// Covers raw access, typed conversion, required access, try-get semantics,
/// presence checks, mutation behavior, well-defined argument guards,
/// and conversion edge cases.
/// </remarks>
[Trait("Category", "Unit")]
public class MetadataExtensionSpec
{
    /// <summary>
    /// Stub enum used by typed conversion tests.
    /// </summary>
    public enum StubMetadataKind
    {
        None = 0,
        Alpha = 1,
        Beta = 2,
    }

    #region Helpers

    /// <summary>
    /// Creates metadata containing one key/value pair.
    /// </summary>
    private static MetadataStub StubWith(
        string key,
        object value)
        => new MetadataStub()
            .With(key, value);

    /// <summary>
    /// Reads a metadata value as <typeparamref name="TValue"/>.
    /// </summary>
    private static TValue? Get<TValue>(
        IMetadata metadata,
        string key)
        => metadata.GetValueOrDefault<TValue>(key);

    /// <summary>
    /// Reads a required metadata value as <typeparamref name="TValue"/>.
    /// </summary>
    private static TValue GetRequired<TValue>(
        IMetadata metadata,
        string key)
        => metadata.GetRequiredValue<TValue>(key);

    #endregion

    #region Raw access

    [Fact]
    public void GetValueOrDefault_When_Key_Present_Should_Return_Raw_Value()
    {
        // Arrange
        IMetadata metadata = StubWith("k", 42);

        // Act
        var actual = metadata.GetValueOrDefault("k");

        // Assert
        actual.ShouldBe(42);
    }

    [Fact]
    public void GetValueOrDefault_When_Key_Missing_Should_Return_Null()
    {
        // Arrange
        IMetadata metadata = new MetadataStub();

        // Act
        var actual = metadata.GetValueOrDefault("k");

        // Assert
        actual.ShouldBeNull();
    }

    [Fact]
    public void GetValueOrDefault_When_Metadata_Null_Should_Throw()
    {
        // Arrange
        IMetadata metadata = null!;

        // Act
        Action act = () => metadata.GetValueOrDefault("k");

        // Assert
        var exception = Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("metadata");
    }

    [Fact]
    public void GetValueOrDefault_When_Key_Null_Should_Throw()
    {
        // Arrange
        IMetadata metadata = new MetadataStub();

        // Act
        Action act = () => metadata.GetValueOrDefault(null!);

        // Assert
        var exception = Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("metadataKey");
        exception.Message.ShouldContain(
            MetadataOutcome.Failure.Key.Argument.Null.Representation);
    }

    #endregion

    #region Typed access

    [Theory]
    [InlineData("42", 42)]
    [InlineData("0", 0)]
    public void GetValueOrDefault_When_String_Represents_Int_Should_Return_Converted(
        string stored,
        int expected)
    {
        // Arrange
        IMetadata metadata = StubWith("k", stored);

        // Act
        var actual = Get<int>(metadata, "k");

        // Assert
        actual.ShouldBe(expected);
    }

    [Fact]
    public void GetValueOrDefault_When_Long_Represents_Int_Should_Return_Converted()
    {
        // Arrange
        IMetadata metadata = StubWith("k", 42L);

        // Act
        var actual = Get<int>(metadata, "k");

        // Assert
        actual.ShouldBe(42);
    }

    [Fact]
    public void GetValueOrDefault_When_Value_Is_Incompatible_Should_Return_Default()
    {
        // Arrange
        IMetadata metadata = StubWith("k", "abc");

        // Act
        var actual = Get<int>(metadata, "k");

        // Assert
        actual.ShouldBe(0);
    }

    [Fact]
    public void GetValueOrDefault_When_Int_Converted_To_String_Should_Return_Invariant_Value()
    {
        // Arrange
        IMetadata metadata = StubWith("k", 42);

        // Act
        var actual = Get<string>(metadata, "k");

        // Assert
        actual.ShouldBe("42");
    }

    [Fact]
    public void GetValueOrDefault_When_Key_Missing_For_Reference_Target_Should_Return_Null()
    {
        // Arrange
        IMetadata metadata = new MetadataStub();

        // Act
        var actual = Get<string>(metadata, "k");

        // Assert
        actual.ShouldBeNull();
    }

    [Fact]
    public void GetValueOrDefault_When_Key_Missing_For_Nullable_Target_Should_Return_Null()
    {
        // Arrange
        IMetadata metadata = new MetadataStub();

        // Act
        var actual = Get<int?>(metadata, "k");

        // Assert
        actual.ShouldBeNull();
    }

    [Fact]
    public void GetValueOrDefault_When_String_Represents_Nullable_Int_Should_Return_Converted()
    {
        // Arrange
        IMetadata metadata = StubWith("k", "5");

        // Act
        var actual = Get<int?>(metadata, "k");

        // Assert
        actual.ShouldBe(5);
    }

    [Fact]
    public void GetValueOrDefault_With_Fallback_When_Key_Missing_Should_Return_Fallback()
    {
        // Arrange
        IMetadata metadata = new MetadataStub();

        // Act
        var actual = metadata.GetValueOrDefault("version", 7);

        // Assert
        actual.ShouldBe(7);
    }

    [Fact]
    public void GetValueOrDefault_With_Fallback_When_Conversion_Fails_Should_Return_Fallback()
    {
        // Arrange
        IMetadata metadata = StubWith("version", "abc");

        // Act
        var actual = metadata.GetValueOrDefault("version", 7);

        // Assert
        actual.ShouldBe(7);
    }

    [Fact]
    public void GetValueOrDefault_With_Fallback_When_Conversion_Succeeds_Should_Return_Converted()
    {
        // Arrange
        IMetadata metadata = StubWith("version", "42");

        // Act
        var actual = metadata.GetValueOrDefault("version", 7);

        // Assert
        actual.ShouldBe(42);
    }

    #endregion

    #region Guid conversion

    [Fact]
    public void GetValueOrDefault_Guid_From_String_Should_Parse()
    {
        // Arrange
        var expected =
            Guid.Parse("11111111-2222-3333-4444-555555555555");

        IMetadata metadata = StubWith("k", expected.ToString());

        // Act
        var actual = Get<Guid>(metadata, "k");

        // Assert
        actual.ShouldBe(expected);
    }

    [Fact]
    public void GetValueOrDefault_Guid_From_Guid_Should_Return_Same_Value()
    {
        // Arrange
        var expected =
            Guid.Parse("11111111-2222-3333-4444-555555555555");

        IMetadata metadata = StubWith("k", expected);

        // Act
        var actual = Get<Guid>(metadata, "k");

        // Assert
        actual.ShouldBe(expected);
    }

    [Fact]
    public void GetValueOrDefault_Guid_From_Invalid_String_Should_Return_Default()
    {
        // Arrange
        IMetadata metadata = StubWith("k", "not-a-guid");

        // Act
        var actual = Get<Guid>(metadata, "k");

        // Assert
        actual.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void GetValueOrDefault_Guid_From_Numeric_Value_Should_Return_Default()
    {
        // Arrange
        IMetadata metadata = StubWith("k", 42);

        // Act
        var actual = Get<Guid>(metadata, "k");

        // Assert
        actual.ShouldBe(Guid.Empty);
    }

    #endregion

    #region DateTime conversion

    [Fact]
    public void GetValueOrDefault_DateTime_From_String_Should_Parse()
    {
        // Arrange
        IMetadata metadata =
            StubWith("k", "2024-06-15T10:30:00Z");

        // Act
        var actual = Get<DateTime>(metadata, "k");

        // Assert
        actual.ShouldBe(
            new DateTime(
                2024,
                6,
                15,
                10,
                30,
                0,
                DateTimeKind.Utc));
    }

    [Fact]
    public void GetValueOrDefault_DateTime_From_DateTimeOffset_Should_Return_Utc()
    {
        // Arrange
        var source =
            new DateTimeOffset(
                2024,
                6,
                15,
                12,
                30,
                0,
                TimeSpan.FromHours(2));

        IMetadata metadata = StubWith("k", source);

        // Act
        var actual = Get<DateTime>(metadata, "k");

        // Assert
        actual.ShouldBe(source.UtcDateTime);
    }

    [Fact]
    public void GetValueOrDefault_DateTime_When_String_Is_Invalid_Should_Return_Default()
    {
        // Arrange
        IMetadata metadata = StubWith("k", "not-a-date");

        // Act
        var actual = Get<DateTime>(metadata, "k");

        // Assert
        actual.ShouldBe(DateTime.MinValue);
    }

    #endregion

    #region DateTimeOffset conversion

    [Fact]
    public void GetValueOrDefault_DateTimeOffset_From_String_Should_Parse()
    {
        // Arrange
        var expected =
            new DateTimeOffset(
                2024,
                6,
                15,
                10,
                30,
                0,
                TimeSpan.FromHours(2));

        IMetadata metadata =
            StubWith("k", "2024-06-15T10:30:00+02:00");

        // Act
        var actual = Get<DateTimeOffset>(metadata, "k");

        // Assert
        actual.ShouldBe(expected);
    }

    [Fact]
    public void GetValueOrDefault_DateTimeOffset_From_DateTime_Should_Use_Zero_Offset()
    {
        // Arrange
        var source =
            new DateTime(
                2024,
                6,
                15,
                10,
                30,
                0,
                DateTimeKind.Utc);

        IMetadata metadata = StubWith("k", source);

        // Act
        var actual = Get<DateTimeOffset>(metadata, "k");

        // Assert
        actual.ShouldBe(new DateTimeOffset(source, TimeSpan.Zero));
    }

    [Fact]
    public void GetValueOrDefault_DateTimeOffset_When_String_Is_Invalid_Should_Return_Default()
    {
        // Arrange
        IMetadata metadata = StubWith("k", "not-a-date");

        // Act
        var actual = Get<DateTimeOffset>(metadata, "k");

        // Assert
        actual.ShouldBe(DateTimeOffset.MinValue);
    }

    #endregion

    #region Enum conversion

    [Theory]
    [InlineData("alpha", StubMetadataKind.Alpha)]
    [InlineData("BETA", StubMetadataKind.Beta)]
    [InlineData("2", StubMetadataKind.Beta)]
    public void GetValueOrDefault_Enum_Should_Parse_Name_Or_Numeric_Value(
        string stored,
        StubMetadataKind expected)
    {
        // Arrange
        IMetadata metadata = StubWith("k", stored);

        // Act
        var actual = Get<StubMetadataKind>(metadata, "k");

        // Assert
        actual.ShouldBe(expected);
    }

    [Fact]
    public void GetValueOrDefault_Enum_From_Numeric_Value_Should_Return_Converted()
    {
        // Arrange
        IMetadata metadata = StubWith("k", 2);

        // Act
        var actual = Get<StubMetadataKind>(metadata, "k");

        // Assert
        actual.ShouldBe(StubMetadataKind.Beta);
    }

    [Fact]
    public void GetValueOrDefault_Enum_When_Name_Is_Invalid_Should_Return_Default()
    {
        // Arrange
        IMetadata metadata = StubWith("k", "Nope");

        // Act
        var actual = Get<StubMetadataKind>(metadata, "k");

        // Assert
        actual.ShouldBe(StubMetadataKind.None);
    }

    [Fact]
    public void GetValueOrDefault_Enum_When_Value_Is_Incompatible_Should_Return_Default()
    {
        // Arrange
        IMetadata metadata = StubWith("k", DateTime.UnixEpoch);

        // Act
        var actual = Get<StubMetadataKind>(metadata, "k");

        // Assert
        actual.ShouldBe(StubMetadataKind.None);
    }

    #endregion

    #region TypeConverter conversion

    [Fact]
    public void GetValueOrDefault_TimeSpan_From_String_Should_Convert()
    {
        // Arrange
        IMetadata metadata = StubWith("k", "01:30:00");

        // Act
        var actual = Get<TimeSpan>(metadata, "k");

        // Assert
        actual.ShouldBe(TimeSpan.FromMinutes(90));
    }

    [Fact]
    public void GetValueOrDefault_Version_From_Empty_String_Should_Return_Null()
    {
        // Arrange
        IMetadata metadata = StubWith("k", string.Empty);

        // Act
        var actual = Get<Version>(metadata, "k");

        // Assert
        actual.ShouldBeNull();
    }

    [Fact]
    public void GetValueOrDefault_Uri_From_Empty_String_Should_Return_Null()
    {
        // Arrange
        IMetadata metadata = StubWith("k", string.Empty);

        // Act
        var actual = Get<Uri>(metadata, "k");

        // Assert
        actual.ShouldBeNull();
    }

    [Fact]
    public void GetValueOrDefault_Uri_From_String_Should_Parse()
    {
        // Arrange
        var expected =
            new Uri("https://example.com/orders/42");

        IMetadata metadata =
            StubWith("k", expected.ToString());

        // Act
        var actual = Get<Uri>(metadata, "k");

        // Assert
        actual.ShouldBe(expected);
    }

    [Fact]
    public void GetValueOrDefault_When_Target_Is_Opaque_Should_Return_Default()
    {
        // Arrange
        IMetadata metadata = StubWith("k", "x");

        // Act
        var actual = Get<StubOpaque>(metadata, "k");

        // Assert
        actual.ShouldBeNull();
    }

    [Fact]
    public void GetValueOrDefault_When_Source_Is_Opaque_And_Target_Is_Int_Should_Return_Default()
    {
        // Arrange
        IMetadata metadata =
            StubWith("k", new StubOpaque());

        // Act
        var actual = Get<int>(metadata, "k");

        // Assert
        actual.ShouldBe(0);
    }

    #endregion

    #region Required access

    [Fact]
    public void GetRequiredValue_When_Key_Present_Should_Return_Raw_Value()
    {
        // Arrange
        IMetadata metadata = StubWith("k", 42);

        // Act
        var actual = metadata.GetRequiredValue("k");

        // Assert
        actual.ShouldBe(42);
    }

    [Fact]
    public void GetRequiredValue_When_Key_Missing_Should_Throw()
    {
        // Arrange
        IMetadata metadata = new MetadataStub();

        // Act
        Action act = () => metadata.GetRequiredValue("k");

        // Assert
        var exception =
            Should.Throw<KeyNotFoundException>(act);

        exception.Message.ShouldContain(
            MetadataOutcome.Failure.Key.Metadata.NotFound.Representation);
    }

    [Fact]
    public void GetRequiredValue_When_Stored_Value_Is_Null_Should_Throw()
    {
        // Arrange
        var dictionary =
            new Dictionary<string, object>
            {
                ["k"] = null!
            };

        IMetadata metadata =
            new MetadataDictionary(dictionary);

        // Act
        Action act = () => metadata.GetRequiredValue("k");

        // Assert
        Should.Throw<KeyNotFoundException>(act);
    }

    [Fact]
    public void GetRequiredValue_When_Value_Is_Convertible_Should_Return_Converted()
    {
        // Arrange
        IMetadata metadata = StubWith("k", "42");

        // Act
        var actual = GetRequired<int>(metadata, "k");

        // Assert
        actual.ShouldBe(42);
    }

    [Fact]
    public void GetRequiredValue_When_Key_Is_Missing_For_Typed_Target_Should_Throw()
    {
        // Arrange
        IMetadata metadata = new MetadataStub();

        // Act
        Action act = () => GetRequired<int>(metadata, "k");

        // Assert
        var exception =
            Should.Throw<KeyNotFoundException>(act);

        exception.Message.ShouldContain(
            MetadataOutcome.Failure.Key.Metadata.NotFound.Representation);
    }

    [Fact]
    public void GetRequiredValue_When_Value_Is_Not_Convertible_Should_Throw()
    {
        // Arrange
        IMetadata metadata = StubWith("k", "abc");

        // Act
        Action act = () => GetRequired<int>(metadata, "k");

        // Assert
        var exception =
            Should.Throw<InvalidCastException>(act);

        exception.Message.ShouldContain(
            MetadataOutcome.Failure.Value.Metadata.InvalidType.Representation);
    }

    [Fact]
    public void GetRequiredValue_When_Conversion_Returns_Null_Should_Throw()
    {
        // Arrange
        IMetadata metadata =
            StubWith("k", new StubOpaque());

        // Act
        Action act = () => GetRequired<string>(metadata, "k");

        // Assert
        Should.Throw<InvalidCastException>(act);
    }

    #endregion

    #region TryGetValue

    [Fact]
    public void TryGetValue_When_Present_And_Convertible_Should_Return_True()
    {
        // Arrange
        IMetadata metadata = StubWith("k", 42);

        // Act
        var found =
            metadata.TryGetValue("k", out int value);

        // Assert
        found.ShouldBeTrue();
        value.ShouldBe(42);
    }

    [Fact]
    public void TryGetValue_When_Present_But_Incompatible_Should_Return_False()
    {
        // Arrange
        IMetadata metadata = StubWith("k", "abc");

        // Act
        var found =
            metadata.TryGetValue("k", out int value);

        // Assert
        found.ShouldBeFalse();
        value.ShouldBe(0);
    }

    [Fact]
    public void TryGetValue_When_Key_Is_Missing_For_Value_Target_Should_Return_False()
    {
        // Arrange
        IMetadata metadata = new MetadataStub();

        // Act
        var found =
            metadata.TryGetValue("k", out int value);

        // Assert
        found.ShouldBeFalse();
        value.ShouldBe(0);
    }

    [Fact]
    public void TryGetValue_When_Key_Is_Missing_For_Reference_Target_Should_Return_False()
    {
        // Arrange
        IMetadata metadata = new MetadataStub();

        // Act
        var found =
            metadata.TryGetValue("k", out string? value);

        // Assert
        found.ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public void TryGetValue_When_Metadata_Is_Null_Should_Throw()
    {
        // Arrange
        IMetadata metadata = null!;

        // Act
        Action act =
            () => metadata.TryGetValue("k", out int _);

        // Assert
        Should.Throw<ArgumentNullException>(act);
    }

    [Fact]
    public void TryGetValue_When_Key_Is_Null_Should_Throw()
    {
        // Arrange
        IMetadata metadata = new MetadataStub();

        // Act
        Action act =
            () => metadata.TryGetValue(null!, out int _);

        // Assert
        var exception =
            Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("metadataKey");
    }

    #endregion

    #region Presence

    [Fact]
    public void Contains_When_Key_Is_Present_Different_Case_Should_Return_True()
    {
        // Arrange
        IMetadata metadata =
            StubWith(
                MetadataConstant.Keyword.CorrelationId,
                "abc");

        // Act
        var actual =
            metadata.Contains("correlationid");

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public void Contains_When_Key_Is_Missing_Should_Return_False()
    {
        // Arrange
        IMetadata metadata = new MetadataStub();

        // Act
        var actual = metadata.Contains("k");

        // Assert
        actual.ShouldBeFalse();
    }

    [Fact]
    public void Contains_When_Metadata_Is_Null_Should_Throw()
    {
        // Arrange
        IMetadata metadata = null!;

        // Act
        Action act = () => metadata.Contains("k");

        // Assert
        Should.Throw<ArgumentNullException>(act);
    }

    [Fact]
    public void Contains_When_Key_Is_Null_Should_Throw()
    {
        // Arrange
        IMetadata metadata = new MetadataStub();

        // Act
        Action act = () => metadata.Contains(null!);

        // Assert
        var exception =
            Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("metadataKey");
    }

    #endregion

    #region Mutation

     [Fact]
     public void SetValue_Should_Add_Entry()
     {
         // Arrange
         IMetadata metadata = MetadataDictionary.Create();
 
         // Act
         metadata.SetValue("k", 1);
 
         // Assert
         metadata["k"].ShouldBe(1);
     }

     [Fact]
     public void SetValue_Should_Overwrite_Entry_Case_Insensitively()
     {
         // Arrange
         IMetadata metadata =
             MetadataDictionary.Create().With("k", 1);
 
         // Act
         metadata.SetValue("K", 2);
 
         // Assert
         metadata.Count.ShouldBe(1);
         metadata["k"].ShouldBe(2);
     }

    [Fact]
    public void SetValue_When_Metadata_Is_Null_Should_Throw()
    {
        // Arrange
        IMetadata metadata = null!;

        // Act
        Action act =
            () => metadata.SetValue("k", 1);

        // Assert
        Should.Throw<ArgumentNullException>(act);
    }

    [Fact]
    public void SetValue_When_Key_Is_Null_Should_Throw()
    {
        // Arrange
        IMetadata metadata = new MetadataStub();

        // Act
        Action act =
            () => metadata.SetValue(null!, 1);

        // Assert
        var exception =
            Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("metadataKey");
    }

    [Fact]
    public void SetValue_When_Value_Is_Null_Should_Throw()
    {
        // Arrange
        IMetadata metadata = new MetadataStub();

        // Act
        Action act =
            () => metadata.SetValue("k", null!);

        // Assert
        var exception =
            Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("value");
    }

    [Fact]
    public void SetValue_When_Metadata_Is_Read_Only_Should_Throw_NotSupported()
    {
        // Arrange
        IMetadata metadata =
            new MetadataStub();

        // Act
        Action act =
            () => metadata.SetValue("k", 1);

        // Assert
        var exception =
            Should.Throw<NotSupportedException>(act);

        exception.Message.ShouldContain(
            MetadataOutcome.Failure.Dictionary.Mutation.NotSupported.Representation);
    }

     [Fact]
     public void Remove_When_Key_Is_Present_Should_Remove_And_Return_True()
     {
         // Arrange
         IMetadata metadata =
             MetadataDictionary.Create().With("k", 1);
 
         // Act
         var removed =
             metadata.Remove("k");
 
         // Assert
         removed.ShouldBeTrue();
         metadata.ContainsKey("k").ShouldBeFalse();
     }

    [Fact]
    public void Remove_When_Key_Is_Missing_Should_Return_False()
    {
        // Arrange
        IMetadata metadata =
            new MetadataStub();

        // Act
        var removed =
            metadata.Remove("k");

        // Assert
        removed.ShouldBeFalse();
    }

    [Fact]
    public void Remove_When_Metadata_Is_Read_Only_Should_Return_False()
    {
        // Arrange
        IMetadata metadata =
            new MetadataStub();

        // Act
        var removed =
            metadata.Remove("k");

        // Assert
        removed.ShouldBeFalse();
    }

    [Fact]
    public void Remove_When_Metadata_Is_Null_Should_Throw()
    {
        // Arrange
        IMetadata metadata = null!;

        // Act
        Action act =
            () => metadata.Remove("k");

        // Assert
        Should.Throw<ArgumentNullException>(act);
    }

    [Fact]
    public void Remove_When_Key_Is_Null_Should_Throw()
    {
        // Arrange
        IMetadata metadata = new MetadataStub();

        // Act
        Action act =
            () => metadata.Remove(null!);

        // Assert
        var exception =
            Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("metadataKey");
    }

    #endregion

    #region Contract

    [Fact]
    public void Metadata_Should_Expose_Read_Only_Dictionary_Contract()
    {
        // Arrange
        IMetadata metadata =
            new MetadataStub();

        // Act

        // Assert
        metadata.ShouldBeAssignableTo<IReadOnlyDictionary<string, object>>();
    }

    #endregion

  
}
