using System.Diagnostics.CodeAnalysis;

using BuildingBlock.Monad.Errors;
using BuildingBlock.Monad.Metadata;

namespace BuildingBlock.Monad.Results;

#region Result

public interface IResult<TError> : IHasMetadata
where TError : IError
{
    #region State

    bool HasValue { get; }
    bool IsSuccess { get; }
    bool IsFailure { get; }
    int StatusCode { get; }

    #endregion

    #region Errors

    List<TError> Errors { get; }

    #endregion
}

#endregion

#region Result<TValue>

public interface IResult<TValue, TError> : IResult<TError>
    where TError : IError
{
    #region Value

    [AllowNull]
    TValue Value { get; }

    bool TryGetValue(
        [MaybeNullWhen(false)] out TValue value);

    #endregion
}
#endregion

