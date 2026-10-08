using System.ComponentModel;
using System.Globalization;

namespace BuildingBlock.Monad.Metadata;

/// <summary>
/// Extension methods for reading, converting, and mutating <see cref="IMetadata"/>.
/// </summary>
public static class MetadataExtensions
{
    #region Raw Access

    /// <summary>
    /// Returns the raw value associated with <paramref name="metadataKey"/>,
    /// or <c>null</c> when the key does not exist.
    /// </summary>
     public static object? GetValueOrDefault(
         this IMetadata metadata,
         string metadataKey)
     {
         ArgumentNullException.ThrowIfNull(metadata);
         if (metadataKey is null)
             throw new ArgumentNullException(
                 "metadataKey",
                 MetadataOutcome.Failure.Key.Argument.Null.Representation);

        return metadata.TryGetValue(metadataKey, out var value)
            ? value
            : null;
    }

    #endregion

    #region Typed Access

    /// <summary>
    /// Returns the converted value associated with <paramref name="metadataKey"/>,
    /// or <c>default</c> when the key is missing or conversion fails.
    /// </summary>
    public static TValue? GetValueOrDefault<TValue>(
        this IMetadata metadata,
        string metadataKey)
    {
        var rawValue = metadata.GetValueOrDefault(metadataKey);

        return TryConvertValue(
            rawValue,
            out TValue? convertedValue)
            ? convertedValue
            : default;
    }

    /// <summary>
    /// Returns the converted value associated with <paramref name="metadataKey"/>,
    /// or <paramref name="fallback"/> when the key is missing or conversion fails.
    /// </summary>
    public static TValue? GetValueOrDefault<TValue>(
        this IMetadata metadata,
        string metadataKey,
        TValue? fallback)
    {
        var rawValue = metadata.GetValueOrDefault(metadataKey);

        return TryConvertValue(
            rawValue,
            out TValue? convertedValue)
            ? convertedValue
            : fallback;
    }

    #endregion

    #region Required Access

    /// <summary>
    /// Returns the raw value associated with <paramref name="metadataKey"/>.
    /// </summary>
    /// <exception cref="KeyNotFoundException">
    /// The metadata key does not exist.
    /// </exception>
    public static object GetRequiredValue(
        this IMetadata metadata,
        string metadataKey)
    {
        var value = metadata.GetValueOrDefault(metadataKey);

        if (value is null)
        {
            throw new KeyNotFoundException(
                MetadataOutcome.Failure.Key.Metadata.NotFound.Representation);
        }

        return value;
    }

    /// <summary>
    /// Returns the converted value associated with <paramref name="metadataKey"/>.
    /// </summary>
    /// <exception cref="KeyNotFoundException">
    /// The metadata key does not exist.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// The metadata value cannot be converted to <typeparamref name="TValue"/>.
    /// </exception>
    public static TValue GetRequiredValue<TValue>(
        this IMetadata metadata,
        string metadataKey)
    {
        var rawValue = metadata.GetValueOrDefault(metadataKey);

        if (rawValue is null)
        {
            throw new KeyNotFoundException(
                MetadataOutcome.Failure.Key.Metadata.NotFound.Representation);
        }

        if (!TryConvertValue(
                rawValue,
                out TValue? convertedValue) ||
            convertedValue is null)
        {
            throw new InvalidCastException(
                MetadataOutcome.Failure.Value.Metadata.InvalidType.Representation);
        }

        return convertedValue;
    }

    #endregion

    #region Try Get

    /// <summary>
    /// Attempts to retrieve and convert the value associated with
    /// <paramref name="metadataKey"/>.
    /// </summary>
    public static bool TryGetValue<TValue>(
        this IMetadata metadata,
        string metadataKey,
        out TValue? value)
    {
        var rawValue = metadata.GetValueOrDefault(metadataKey);

        if (rawValue is null)
        {
            value = default;
            return false;
        }

        return TryConvertValue(rawValue, out value);
    }

    #endregion

    #region Presence

    /// <summary>
    /// Determines whether <paramref name="metadataKey"/> exists.
    /// </summary>
    public static bool Contains(
        this IMetadata metadata,
        string metadataKey)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(metadataKey);

        return metadata.ContainsKey(metadataKey);
    }

    #endregion

    #region Mutation

    /// <summary>
    /// Adds or replaces a metadata value.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// The underlying metadata implementation does not support mutation.
    /// </exception>
    public static void SetValue(
        this IMetadata metadata,
        string metadataKey,
        object value)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(metadataKey);
        ArgumentNullException.ThrowIfNull(value);

        if (metadata is IDictionary<string, object> mutable)
        {
            mutable[metadataKey] = value;
            return;
        }

        throw new NotSupportedException(
            MetadataOutcome.Failure.Dictionary.Mutation.NotSupported.Representation);
    }

    /// <summary>
    /// Removes a metadata value.
    /// </summary>
    /// <returns>
    /// <c>true</c> when the value was found and removed; otherwise <c>false</c>.
    /// </returns>
    public static bool Remove(
        this IMetadata metadata,
        string metadataKey)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(metadataKey);

        return metadata is IDictionary<string, object> mutable &&
               mutable.Remove(metadataKey);
    }

    #endregion

    #region Conversion

    internal static bool TryConvertValue<TValue>(
        object? value,
        out TValue? result)
    {
        result = default;

        if (value is null)
        {
            return default(TValue) is null;
        }

        if (value is TValue direct)
        {
            result = direct;
            return true;
        }

        var targetType =
            Nullable.GetUnderlyingType(typeof(TValue))
            ?? typeof(TValue);

        var sourceType = value.GetType();

        if (targetType == typeof(string))
        {
            result = (TValue)(object)
                Convert.ToString(
                    value,
                    CultureInfo.InvariantCulture)!;

            return true;
        }

        if (targetType == typeof(Guid))
        {
            if (value is string guidText &&
                Guid.TryParse(guidText, out var guid))
            {
                result = (TValue)(object)guid;
                return true;
            }

            return false;
        }

        if (targetType == typeof(DateTimeOffset))
        {
            if (value is string dateText &&
                DateTimeOffset.TryParse(
                    dateText,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var dateTimeOffset))
            {
                result = (TValue)(object)dateTimeOffset;
                return true;
            }

            if (value is DateTime dateTime)
            {
                result = (TValue)(object)new DateTimeOffset(
                    dateTime,
                    TimeSpan.Zero);

                return true;
            }

            return false;
        }

        if (targetType == typeof(DateTime))
        {
            if (value is string dateText &&
                DateTime.TryParse(
                    dateText,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var dateTime))
            {
                result = (TValue)(object)dateTime;
                return true;
            }

            if (value is DateTimeOffset dateTimeOffset)
            {
                result = (TValue)(object)dateTimeOffset.UtcDateTime;
                return true;
            }

            return false;
        }

        if (targetType.IsEnum)
        {
            try
            {
                if (value is string enumName)
                {
                    result = (TValue)Enum.Parse(
                        targetType,
                        enumName,
                        ignoreCase: true);

                    return true;
                }

                result = (TValue)Enum.ToObject(
                    targetType,
                    value);

                return true;
            }
            catch
            {
                return false;
            }
        }

        if (value is IConvertible &&
            typeof(IConvertible).IsAssignableFrom(targetType))
        {
            try
            {
                var converted = Convert.ChangeType(
                    value,
                    targetType,
                    CultureInfo.InvariantCulture);

                result = (TValue)converted!;
                return true;
            }
            catch
            {
                // Fall through to TypeConverter.
            }
        }

        try
        {
            var converter = TypeDescriptor.GetConverter(targetType);

            if (!converter.CanConvertFrom(sourceType))
            {
                return false;
            }

            var converted = converter.ConvertFrom(
                context: null,
                culture: CultureInfo.InvariantCulture,
                value: value);

            if (converted is null)
            {
                return false;
            }

            result = (TValue)converted;
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
