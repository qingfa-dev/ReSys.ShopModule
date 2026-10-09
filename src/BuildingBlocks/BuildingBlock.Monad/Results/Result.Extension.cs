using BuildingBlock.Monad.Errors;
using BuildingBlock.Monad.Metadata;

namespace BuildingBlock.Monad.Results;

public static partial class ResultExtension
{
    #region Metadata

    public static Result WithMetadata(
        this Result result,
        string key,
        object value)
    {
        ArgumentNullException.ThrowIfNull(result);

        var metadata = MetadataDictionary.Create(result.Metadata);

        metadata.SetValue(key, value);

        return result.CopyWith(
            metadata: metadata);
    }

    public static Result<TValue> WithMetadata<TValue>(
        this Result<TValue> result,
        string key,
        object value)
    {
        ArgumentNullException.ThrowIfNull(result);

        var metadata = MetadataDictionary.Create(result.Metadata);

        metadata.SetValue(key, value);

        return result.CopyWith(
            metadata: metadata);
    }

    public static Result WithMetadata(
        this Result result,
        MetadataDictionary metadata)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(metadata);

        return result.CopyWith(
            metadata: MetadataDictionary.Create(metadata));
    }

    public static Result<TValue> WithMetadata<TValue>(
        this Result<TValue> result,
        MetadataDictionary metadata)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(metadata);

        return result.CopyWith(
            metadata: MetadataDictionary.Create(metadata));
    }

    #endregion

    #region Standard Metadata

    public static Result WithTraceId(
        this Result result,
        object value)
        => result.WithMetadata(
            ErrorConstant.Metadata.TraceId,
            value);

    public static Result<TValue> WithTraceId<TValue>(
        this Result<TValue> result,
        object value)
        => result.WithMetadata(
            ErrorConstant.Metadata.TraceId,
            value);

    public static Result WithTimestamp(
        this Result result,
        object value)
        => result.WithMetadata(
            ErrorConstant.Metadata.Timestamp,
            value);

    public static Result<TValue> WithTimestamp<TValue>(
        this Result<TValue> result,
        object value)
        => result.WithMetadata(
            ErrorConstant.Metadata.Timestamp,
            value);

    public static Result WithResource(
        this Result result,
        object value)
        => result.WithMetadata(
            ErrorConstant.Metadata.Resource,
            value);

    public static Result<TValue> WithResource<TValue>(
        this Result<TValue> result,
        object value)
        => result.WithMetadata(
            ErrorConstant.Metadata.Resource,
            value);

    public static Result WithField(
        this Result result,
        object value)
        => result.WithMetadata(
            ErrorConstant.Metadata.Field,
            value);

    public static Result<TValue> WithField<TValue>(
        this Result<TValue> result,
        object value)
        => result.WithMetadata(
            ErrorConstant.Metadata.Field,
            value);

    public static Result WithAttempt(
        this Result result,
        object value)
        => result.WithMetadata(
            ErrorConstant.Metadata.Attempt,
            value);

    public static Result<TValue> WithAttempt<TValue>(
        this Result<TValue> result,
        object value)
        => result.WithMetadata(
            ErrorConstant.Metadata.Attempt,
            value);

    #endregion

    #region Status

    public static Result WithStatus(
        this Result result,
        int statusCode)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.CopyWith(
            statusCode: statusCode);
    }

    public static Result<TValue> WithStatus<TValue>(
        this Result<TValue> result,
        int statusCode)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.CopyWith(
            statusCode: statusCode);
    }

    #endregion

    #region Internal Propagation

    internal static Result WithMetadataFrom(
        this Result result,
        Result source)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(source);

        return result.CopyWith(
            metadata: MetadataDictionary.Create(
                source.Metadata));
    }

    internal static Result<TValue> WithMetadataFrom<TValue>(
        this Result<TValue> result,
        Result source)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(source);

        return result.CopyWith(
            metadata: MetadataDictionary.Create(
                source.Metadata));
    }

    #endregion
}