namespace BuildingBlock.Monad.Metadata;

/// <summary>
/// Extension methods for objects implementing
/// <see cref="IHasMetadata{TMetadata}"/>.
/// </summary>
public static class HasMetadataExtensions
{
    #region Raw Access

    /// <summary>
    /// Returns the raw metadata value associated with
    /// <paramref name="metadataKey"/>.
    /// </summary>
    public static object? GetValueOrDefault<TMetadata>(
        this IHasMetadata<TMetadata> source,
        string metadataKey)
        where TMetadata : IMetadata
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Metadata.GetValueOrDefault(metadataKey);
    }

    #endregion

    #region Typed Access

    /// <summary>
    /// Returns the converted metadata value associated with
    /// <paramref name="metadataKey"/>.
    /// </summary>
    public static TValue? GetValueOrDefault<TMetadata, TValue>(
        this IHasMetadata<TMetadata> source,
        string metadataKey)
        where TMetadata : IMetadata
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Metadata.GetValueOrDefault<TValue>(metadataKey);
    }

    /// <summary>
    /// Returns the converted metadata value or
    /// <paramref name="fallback"/> when unavailable.
    /// </summary>
    public static TValue? GetValueOrDefault<TMetadata, TValue>(
        this IHasMetadata<TMetadata> source,
        string metadataKey,
        TValue? fallback)
        where TMetadata : IMetadata
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Metadata.GetValueOrDefault(
            metadataKey,
            fallback);
    }

    #endregion

    #region Required Access

    /// <summary>
    /// Returns the required raw metadata value.
    /// </summary>
    public static object GetRequiredValue<TMetadata>(
        this IHasMetadata<TMetadata> source,
        string metadataKey)
        where TMetadata : IMetadata
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Metadata.GetRequiredValue(metadataKey);
    }

    /// <summary>
    /// Returns the required converted metadata value.
    /// </summary>
    public static TValue GetRequiredValue<TMetadata, TValue>(
        this IHasMetadata<TMetadata> source,
        string metadataKey)
        where TMetadata : IMetadata
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Metadata.GetRequiredValue<TValue>(
            metadataKey);
    }

    #endregion

    #region Try Get

    /// <summary>
    /// Attempts to retrieve and convert a metadata value.
    /// </summary>
    public static bool TryGetValue<TMetadata, TValue>(
        this IHasMetadata<TMetadata> source,
        string metadataKey,
        out TValue? value)
        where TMetadata : IMetadata
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Metadata.TryGetValue(
            metadataKey,
            out value);
    }

    #endregion

    #region Presence

    /// <summary>
    /// Determines whether the metadata contains
    /// <paramref name="metadataKey"/>.
    /// </summary>
    public static bool Contains<TMetadata>(
        this IHasMetadata<TMetadata> source,
        string metadataKey)
        where TMetadata : IMetadata
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Metadata.Contains(metadataKey);
    }

    #endregion

    #region Mutation

    /// <summary>
    /// Adds or replaces a metadata value.
    /// </summary>
    public static void SetValue<TMetadata>(
        this IHasMetadata<TMetadata> source,
        string metadataKey,
        object value)
        where TMetadata : IMetadata
    {
        ArgumentNullException.ThrowIfNull(source);

        source.Metadata.SetValue(
            metadataKey,
            value);
    }

    /// <summary>
    /// Removes a metadata value.
    /// </summary>
    public static bool Remove<TMetadata>(
        this IHasMetadata<TMetadata> source,
        string metadataKey)
        where TMetadata : IMetadata
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Metadata.Remove(metadataKey);
    }

    #endregion

    #region Well-Known Metadata

    /// <summary>
    /// Gets the correlation ID.
    /// </summary>
    public static Guid? GetCorrelationId<TMetadata>(
        this IHasMetadata<TMetadata> source)
        where TMetadata : IMetadata
        => source.Metadata.GetValueOrDefault<Guid?>(
            MetadataConstant.Keyword.CorrelationId);

    /// <summary>
    /// Gets the request ID.
    /// </summary>
    public static Guid? GetRequestId<TMetadata>(
        this IHasMetadata<TMetadata> source)
        where TMetadata : IMetadata
        => source.Metadata.GetValueOrDefault<Guid?>(
            MetadataConstant.Keyword.RequestId);

    /// <summary>
    /// Gets the causation ID.
    /// </summary>
    public static Guid? GetCausationId<TMetadata>(
        this IHasMetadata<TMetadata> source)
        where TMetadata : IMetadata
        => source.Metadata.GetValueOrDefault<Guid?>(
            MetadataConstant.Keyword.CausationId);

    /// <summary>
    /// Gets the timestamp.
    /// </summary>
    public static DateTimeOffset? GetTimestamp<TMetadata>(
        this IHasMetadata<TMetadata> source)
        where TMetadata : IMetadata
        => source.Metadata.GetValueOrDefault<DateTimeOffset?>(
            MetadataConstant.Keyword.Timestamp);

    #endregion
}
