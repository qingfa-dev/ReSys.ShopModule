using BuildingBlock.Monad.Errors;
using BuildingBlock.Monad.Metadata;

namespace BuildingBlock.Monad.Results;

public partial record Result : IResult<Error>
{
    #region Properties

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public bool HasValue => IsSuccess;

    public List<Error> Errors { get; }

    public MetadataDictionary Metadata { get; protected set; } = new();

    public int StatusCode { get; }

    #endregion

    #region Constructors

    public Result(
        bool isSuccess,
        List<Error>? errors = null,
        int? statusCode = null)
    {
        var resolvedErrors = errors ?? ResultConstant.Default.EmptyErrors;

        var resolvedStatusCode =
            statusCode
            ?? ResolveDefaultStatusCode(isSuccess, resolvedErrors);

        var error =
            ResultGuard.ValidateStatusCode(
                resolvedStatusCode,
                isSuccess)
            ?? ResultGuard.ValidateErrors(
                resolvedErrors,
                isSuccess)
            ?? ResultGuard.ValidateResultConsistency(
                isSuccess,
                resolvedErrors);

        if (error is not null)
        {
            throw new ArgumentException(
                error.ToString(),
                nameof(errors));
        }

        IsSuccess = isSuccess;
        Errors = resolvedErrors.ToList();
        StatusCode = resolvedStatusCode;
    }

    #endregion

    #region Copy

    public Result CopyWith(
        bool? isSuccess = null,
        List<Error>? errors = null,
        int? statusCode = null,
        MetadataDictionary? metadata = null)
    {
        var resolvedIsSuccess = isSuccess ?? IsSuccess;

        var resolvedErrors = errors is not null
            ? errors.ToList()
            : resolvedIsSuccess == IsSuccess
                ? Errors.ToList()
                : [];

        var resolvedStatusCode =
            statusCode
            ?? ResolveDefaultStatusCode(
                resolvedIsSuccess,
                resolvedErrors);

        return new Result(
            isSuccess: resolvedIsSuccess,
            errors: resolvedErrors,
            statusCode: resolvedStatusCode)
        {
            Metadata = MetadataDictionary.Create(
                metadata ?? Metadata)
        };
    }

    #endregion

    #region Equality

    public virtual bool Equals(Result? other)
        => other is not null
        && IsSuccess == other.IsSuccess
        && StatusCode == other.StatusCode
        && Errors.SequenceEqual(other.Errors);

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(IsSuccess);
        hash.Add(StatusCode);

        foreach (var error in Errors)
        {
            hash.Add(error);
        }

        return hash.ToHashCode();
    }

    #endregion

    #region ToString

    public override string ToString()
        => IsSuccess
            ? $"Success ({StatusCode})"
            : $"Failure ({StatusCode}): {string.Join(", ", Errors)}";

    #endregion

    #region Helpers

    protected static int ResolveDefaultStatusCode(
        bool isSuccess,
        List<Error> errors)
    {
        if (isSuccess)
        {
            return ResultConstant.StatusCode.Ok;
        }

        if (errors.Count == 0)
        {
            return ResultConstant.Default.FailureStatus;
        }

        var statusCode = errors[0].Status;

        return statusCode
            ?? ResultConstant.Default.FailureStatus;
    }

    #endregion
}