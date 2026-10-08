using BuildingBlock.Monad.Metadata;

namespace BuildingBlock.Monad.UnitTest.Metadata;

/// <summary>
/// Tests for <see cref="MetadataDictionary"/> construction, factories,
/// empty state, and fluent mutation.
/// </summary>
/// <remarks>
/// Verifies case-insensitive key behavior, defensive dictionary copying,
/// pair initialization, duplicate-key handling, null argument guards,
/// shared empty-instance behavior, and fluent mutation.
/// </remarks>
[Trait("Category", "Unit")]
public class MetadataDictionarySpec
{
    #region Constructors

    [Fact]
    public void New_Should_Create_Empty_Case_Insensitive_Store()
    {
        // Arrange
        var dictionary = new MetadataDictionary();

        // Act
        dictionary["Alpha"] = 1;

        // Assert
        dictionary.Count.ShouldBe(1);
        dictionary.ContainsKey("ALPHA").ShouldBeTrue();
        dictionary["alpha"].ShouldBe(1);
    }

    [Fact]
    public void New_From_Dictionary_Should_Copy_Entries()
    {
        // Arrange
        var source = new Dictionary<string, object>
        {
            ["CorrelationId"] = "abc"
        };

        // Act
        var dictionary = new MetadataDictionary(source);

        // Assert
        dictionary.Count.ShouldBe(1);
        dictionary["correlationid"].ShouldBe("abc");
    }

    [Fact]
    public void New_From_Dictionary_Should_Not_Share_Source()
    {
        // Arrange
        var source = new Dictionary<string, object>
        {
            ["CorrelationId"] = "abc"
        };

        // Act
        var dictionary = new MetadataDictionary(source);
        source["CorrelationId"] = "changed";

        // Assert
        dictionary["CorrelationId"].ShouldBe("abc");
    }

    [Fact]
    public void New_From_Dictionary_When_Null_Should_Throw()
    {
        // Arrange
        IDictionary<string, object> source = null!;

        // Act
        var act = () => new MetadataDictionary(source);

        // Assert
        var exception = Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("dictionary");
        exception.Message.ShouldContain(
            MetadataOutcome.Failure.Dictionary.Argument.Null.Representation);
    }

    [Fact]
    public void New_From_Pairs_When_Null_Should_Throw()
    {
        // Arrange
        IEnumerable<KeyValuePair<string, object>> pairs = null!;

        // Act
        var act = () => new MetadataDictionary(pairs);

        // Assert
        var exception = Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("pairs");
        exception.Message.ShouldContain(
            MetadataOutcome.Failure.Dictionary.Argument.Null.Representation);
    }

    [Fact]
    public void New_From_Pairs_Should_Create_Store()
    {
        // Arrange
        var pairs = new[]
        {
            new KeyValuePair<string, object>("CorrelationId", "abc"),
            new KeyValuePair<string, object>("RequestId", "def")
        };

        // Act
        var dictionary = new MetadataDictionary(pairs);

        // Assert
        dictionary.Count.ShouldBe(2);
        dictionary["correlationid"].ShouldBe("abc");
        dictionary["requestid"].ShouldBe("def");
    }

    [Fact]
    public void New_From_Pairs_When_Key_Null_Should_Throw()
    {
        // Arrange
        var pairs = new[]
        {
            new KeyValuePair<string, object>(null!, "value")
        };

        // Act
        var act = () => new MetadataDictionary(pairs);

        // Assert
        var exception = Should.Throw<ArgumentException>(act);

        exception.ParamName.ShouldBe("pairs");
        exception.Message.ShouldContain(
            MetadataOutcome.Failure.Key.Argument.Null.Representation);
    }

    [Fact]
    public void New_From_Pairs_When_Key_Duplicated_Should_Keep_Last_Value()
    {
        // Arrange
        var pairs = new[]
        {
            new KeyValuePair<string, object>("a", 1),
            new KeyValuePair<string, object>("a", 2)
        };

        // Act
        var dictionary = new MetadataDictionary(pairs);

        // Assert
        dictionary.Count.ShouldBe(1);
        dictionary["a"].ShouldBe(2);
    }

    [Fact]
    public void New_From_Pairs_When_Empty_Should_Create_Empty_Store()
    {
        // Arrange
        var pairs = Array.Empty<KeyValuePair<string, object>>();

        // Act
        var dictionary = new MetadataDictionary(pairs);

        // Assert
        dictionary.ShouldBeEmpty();
    }

    #endregion

    #region Factory methods

    [Fact]
    public void Create_Should_Return_Fresh_Instance()
    {
        // Arrange

        // Act
        var first = MetadataDictionary.Create();
        var second = MetadataDictionary.Create();

        // Assert
        first.ShouldNotBeSameAs(second);
        first.ShouldNotBeSameAs(MetadataDictionary.Empty);
        second.ShouldNotBeSameAs(MetadataDictionary.Empty);
    }

    [Fact]
    public void Create_From_Dictionary_Should_Copy_Entries()
    {
        // Arrange
        var source = new Dictionary<string, object>
        {
            ["RequestId"] = 1
        };

        // Act
        var dictionary = MetadataDictionary.Create(source);

        // Assert
        dictionary.Count.ShouldBe(1);
        dictionary["requestid"].ShouldBe(1);
    }

    [Fact]
    public void Create_From_Dictionary_When_Null_Should_Throw()
    {
        // Arrange
        IDictionary<string, object> source = null!;

        // Act
        var act = () => MetadataDictionary.Create(source);

        // Assert
        var exception = Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("dictionary");
        exception.Message.ShouldContain(
            MetadataOutcome.Failure.Dictionary.Argument.Null.Representation);
    }

    [Fact]
    public void Create_From_Pairs_When_Null_Should_Throw()
    {
        // Arrange
        IEnumerable<KeyValuePair<string, object>> pairs = null!;

        // Act
        var act = () => MetadataDictionary.Create(pairs);

        // Assert
        var exception = Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("pairs");
        exception.Message.ShouldContain(
            MetadataOutcome.Failure.Dictionary.Argument.Null.Representation);
    }

    #endregion

    #region Empty

    [Fact]
    public void Empty_Should_Be_Shared_Instance()
    {
        // Arrange

        // Act
        var first = MetadataDictionary.Empty;
        var second = MetadataDictionary.Empty;

        // Assert
        first.ShouldBeSameAs(second);
        first.ShouldBeEmpty();
    }

    #endregion

    #region With

    [Fact]
    public void With_Should_Return_Same_Instance()
    {
        // Arrange
        var dictionary = MetadataDictionary.Create();

        // Act
        var result = dictionary.With("a", 1);

        // Assert
        result.ShouldBeSameAs(dictionary);
        dictionary["a"].ShouldBe(1);
    }

    [Fact]
    public void With_Should_Overwrite_Case_Insensitively()
    {
        // Arrange
        var dictionary = MetadataDictionary.Create()
            .With("alpha", 1);

        // Act
        dictionary.With("ALPHA", 2);

        // Assert
        dictionary.Count.ShouldBe(1);
        dictionary["Alpha"].ShouldBe(2);
    }

    [Fact]
    public void With_When_Key_Null_Should_Throw()
    {
        // Arrange
        var dictionary = MetadataDictionary.Create();

        // Act
        var act = () => dictionary.With(null!, 1);

        // Assert
        var exception = Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("key");
        exception.Message.ShouldContain(
            MetadataOutcome.Failure.Key.Argument.Null.Representation);
    }

    [Fact]
    public void With_When_Value_Null_Should_Throw()
    {
        // Arrange
        var dictionary = MetadataDictionary.Create();

        // Act
        var act = () => dictionary.With("a", null!);

        // Assert
        var exception = Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("value");
    }

    #endregion

    #region Contract

    [Fact]
    public void Metadata_Should_Implement_Metadata_Contract()
    {
        // Arrange
        var dictionary = new MetadataDictionary();

        // Act

        // Assert
        dictionary.ShouldBeAssignableTo<IMetadata>();
        dictionary.ShouldBeAssignableTo<IReadOnlyDictionary<string, object>>();
    }

    #endregion
}
