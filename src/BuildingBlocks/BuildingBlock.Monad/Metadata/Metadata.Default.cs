namespace BuildingBlock.Monad.Metadata;

public sealed class MetadataDictionary : Dictionary<string, object>, IMetadata
{
    #region Constructors

    public MetadataDictionary()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    public MetadataDictionary(IDictionary<string, object> dictionary)
        : base(
            EnsureNotNull(dictionary),
            StringComparer.OrdinalIgnoreCase)
    {
    }

    public MetadataDictionary(
        IEnumerable<KeyValuePair<string, object>> pairs)
        : base(StringComparer.OrdinalIgnoreCase)
    {
        if (pairs is null)
            throw new ArgumentNullException(
                nameof(pairs),
                MetadataOutcome.Failure.Dictionary.Argument.Null.Representation);

        foreach (var pair in pairs)
        {
            if (pair.Key is null)
            {
                throw new ArgumentException(
                    MetadataOutcome.Failure.Key.Argument.Null.Representation,
                    nameof(pairs));
            }

            this[pair.Key] = pair.Value;
        }
    }

    #endregion

    #region Factory Methods

    public static MetadataDictionary Create()
        => new();

    public static MetadataDictionary Create(
        IDictionary<string, object> dictionary)
        => new(dictionary);

    public static MetadataDictionary Create(
        IEnumerable<KeyValuePair<string, object>> pairs)
        => new(pairs);

    public static MetadataDictionary Empty { get; } = new();

    public MetadataDictionary With(string key, object value)
    {
        if (key is null)
            throw new ArgumentNullException(
                nameof(key),
                MetadataOutcome.Failure.Key.Argument.Null.Representation);

        ArgumentNullException.ThrowIfNull(value);

        this[key] = value;

        return this;
    }

    #endregion

    #region Helpers

    private static IDictionary<string, object> EnsureNotNull(
        IDictionary<string, object> dictionary)
    {
        if (dictionary is null)
            throw new ArgumentNullException(
                nameof(dictionary),
                MetadataOutcome.Failure.Dictionary.Argument.Null.Representation);

        return dictionary;
    }

    #endregion
}