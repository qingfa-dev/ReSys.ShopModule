using System.Diagnostics.CodeAnalysis;

using BuildingBlock.Monad.Errors;
using BuildingBlock.Monad.Metadata;

namespace BuildingBlock.Monad.Results;

public partial record Result<TValue>
    : Result,
      IResult<TValue, Error>

{
    #region Value

    [AllowNull]
    public TValue Value { get; }

    #endregion

    #region Constructor

    public Result(
        bool isSuccess,
        TValue value,
        List<Error>? errors = null,
        int? statusCode = null)
        : base(
            isSuccess: isSuccess,
            errors: errors ?? new List<Error>(),
            statusCode: statusCode)
    {
        var valueError = ResultGuard.ValidateValue(
            isSuccess,
            value);

        if (valueError is not null)
        {
            throw new ArgumentException(
                valueError.ToString(),
                nameof(value));
        }

        Value = value;
    }

    #endregion

    #region Copy

    /// <summary>
    /// Creates a copy of the current result.
    /// </summary>
    /// <remarks>
    /// A failed result cannot become successful without supplying a value.
    /// Use the overload accepting <paramref name="value"/> to provide one.
    /// </remarks>
    public new Result<TValue> CopyWith(
        bool? isSuccess = null,
        List<Error>? errors = null,
        int? statusCode = null,
        MetadataDictionary? metadata = null)
    {
        var resolvedIsSuccess = isSuccess ?? IsSuccess;

        if (resolvedIsSuccess && !IsSuccess)
        {
            throw new ArgumentException(
                ResultOutcome.Failure.Value.Required.ToString(),
                nameof(isSuccess));
        }

        List<Error> resolvedErrors = errors is not null
            ? errors.ToList()
            : Errors.ToList();

        var resolvedStatusCode = statusCode
            ?? (resolvedIsSuccess == IsSuccess
                ? StatusCode
                : Result.ResolveDefaultStatusCode(
                    resolvedIsSuccess,
                    resolvedErrors));

        return new Result<TValue>(
            isSuccess: resolvedIsSuccess,
            value: Value,
            errors: resolvedErrors,
            statusCode: resolvedStatusCode)
        {
            Metadata = MetadataDictionary.Create(
                metadata ?? Metadata)
        };
    }

    /// <summary>
    /// Creates a copy of the current result with the specified value.
    /// </summary>
    public Result<TValue> CopyWith(
        TValue value,
        bool? isSuccess = null,
        List<Error>? errors = null,
        int? statusCode = null,
        MetadataDictionary? metadata = null)
    {
        var resolvedIsSuccess = isSuccess ?? IsSuccess;

        List<Error> resolvedErrors = errors is not null
            ? [.. errors]
            : resolvedIsSuccess
                ? new List<Error>()
                : [.. Errors];

        var resolvedStatusCode = statusCode
            ?? (resolvedIsSuccess == IsSuccess
                ? StatusCode
                : Result.ResolveDefaultStatusCode(
                    resolvedIsSuccess,
                    resolvedErrors));

        return new Result<TValue>(
            isSuccess: resolvedIsSuccess,
            value: value,
            errors: resolvedErrors,
            statusCode: resolvedStatusCode)
        {
            Metadata = MetadataDictionary.Create(
                metadata ?? Metadata)
        };
    }

    #endregion

    #region Value Access

    public bool TryGetValue(
        [MaybeNullWhen(false)] out TValue value)
    {
        if (IsSuccess)
        {
            value = Value;
            return true;
        }

        value = default;
        return false;
    }

    #endregion

    #region ToString

    public override string ToString()
        => IsSuccess
            ? $"Success ({StatusCode}): {Value}"
            : $"Failure ({StatusCode}): {string.Join(", ", Errors)}";

    #endregion
}