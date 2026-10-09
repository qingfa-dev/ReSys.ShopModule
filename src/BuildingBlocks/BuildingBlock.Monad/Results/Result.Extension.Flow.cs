using BuildingBlock.Monad.Errors;

namespace BuildingBlock.Monad.Results;

public static partial class ResultExtension
{
    #region Pattern Matching

    public static TOut? Match<TOut>(
        this Result result,
        Func<TOut>? onSuccess,
        Func<List<Error>, TOut>? onFailure)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
            return onSuccess is null
                ? default
                : onSuccess();

        return onFailure is null
            ? default
            : onFailure(result.Errors);
    }

    public static TOut? Fold<TOut>(
        this Result result,
        Func<TOut>? onSuccess,
        Func<List<Error>, TOut>? onFailure)
        => result.Match(
            onSuccess,
            onFailure);

    #endregion

    #region Map

    public static Result<TOut> Map<TOut>(
        this Result result,
        Func<TOut>? mapper)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
        {
            return Result<TOut>.Fail(
                result.Errors,
                result.StatusCode)
                .WithMetadataFrom(result);
        }

        if (mapper is null)
        {
            return Result<TOut>.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(nameof(Map)));
        }

        return Result<TOut>.Ok(
                mapper(),
                result.StatusCode)
            .WithMetadataFrom(result);
    }

    public static Result<TOut> Map<TValue, TOut>(
        this Result<TValue> result,
        Func<TValue, TOut>? mapper)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
        {
            return Result<TOut>.Fail(
                result.Errors,
                result.StatusCode)
                .WithMetadataFrom(result);
        }

        if (mapper is null)
        {
            return Result<TOut>.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(nameof(Map)));
        }

        return Result<TOut>.Ok(
                mapper(result.Value),
                result.StatusCode)
            .WithMetadataFrom(result);
    }

    #endregion

    #region MapError

    public static Result MapError(
        this Result result,
        Func<Error, Error>? mapper)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
            return result;

        if (mapper is null)
        {
            return Result.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(MapError)));
        }

        var errors = new List<Error>(
            result.Errors.Count);

        foreach (var error in result.Errors)
        {
            var mapped = mapper(error);

            if (mapped is null)
            {
                return Result.Fail(
                    ResultOutcome.Failure.Flow.ErrorMissing(
                        nameof(MapError)));
            }

            errors.Add(mapped);
        }

        return Result.Failure(errors)
            .WithMetadataFrom(result);
    }

    public static Result<TValue> MapError<TValue>(
        this Result<TValue> result,
        Func<Error, Error>? mapper)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
            return result;

        if (mapper is null)
        {
            return Result<TValue>.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(MapError)));
        }

        var errors = new List<Error>(
            result.Errors.Count);

        foreach (var error in result.Errors)
        {
            var mapped = mapper(error);

            if (mapped is null)
            {
                return Result<TValue>.Fail(
                    ResultOutcome.Failure.Flow.ErrorMissing(
                        nameof(MapError)));
            }

            errors.Add(mapped);
        }

        return Result<TValue>.Fail(errors)
            .WithMetadataFrom(result);
    }

    #endregion

    #region Bind

    public static Result<TOut> Bind<TOut>(
        this Result result,
        Func<Result<TOut>>? binder)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
        {
            return Result<TOut>.Fail(
                result.Errors,
                result.StatusCode)
                .WithMetadataFrom(result);
        }

        if (binder is null)
        {
            return Result<TOut>.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(Bind)));
        }

        var bound = binder();

        if (bound is null)
        {
            return Result<TOut>.Fail(
                ResultOutcome.Failure.Flow.ResultMissing(
                    nameof(Bind)));
        }

        return bound.WithMetadataFrom(result);
    }

    public static Result<TOut> Bind<TValue, TOut>(
        this Result<TValue> result,
        Func<TValue, Result<TOut>>? binder)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
        {
            return Result<TOut>.Fail(
                result.Errors,
                result.StatusCode)
                .WithMetadataFrom(result);
        }

        if (binder is null)
        {
            return Result<TOut>.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(Bind)));
        }

        var bound = binder(result.Value);

        if (bound is null)
        {
            return Result<TOut>.Fail(
                ResultOutcome.Failure.Flow.ResultMissing(
                    nameof(Bind)));
        }

        return bound.WithMetadataFrom(result);
    }

    #endregion

    #region Flatten

    public static Result<TValue> Flatten<TValue>(
        this Result<Result<TValue>> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
        {
            return Result<TValue>.Fail(
                result.Errors,
                result.StatusCode)
                .WithMetadataFrom(result);
        }

        var inner = result.Value;

        if (inner is null)
        {
            return Result<TValue>.Fail(
                ResultOutcome.Failure.Flow.ResultMissing(
                    nameof(Flatten)));
        }

        return inner.WithMetadataFrom(result);
    }

    #endregion

    #region Ensure

    public static Result Ensure(
        this Result result,
        Func<bool>? predicate,
        Error? error)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
            return result;

        if (predicate is null)
        {
            return Result.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(Ensure)));
        }

        if (error is null)
        {
            return Result.Fail(
                ResultOutcome.Failure.Flow.ErrorMissing(
                    nameof(Ensure)));
        }

        return predicate()
            ? result
            : Result.Fail(error)
                .WithMetadataFrom(result);
    }

    public static Result<TValue> Ensure<TValue>(
        this Result<TValue> result,
        Func<TValue, bool>? predicate,
        Error? error)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
            return result;

        if (predicate is null)
        {
            return Result<TValue>.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(Ensure)));
        }

        if (error is null)
        {
            return Result<TValue>.Fail(
                ResultOutcome.Failure.Flow.ErrorMissing(
                    nameof(Ensure)));
        }

        return predicate(result.Value)
            ? result
            : Result<TValue>.Fail(error)
                .WithMetadataFrom(result);
    }

    #endregion

    #region Recover

    public static Result Recover(
        this Result result,
        Func<List<Error>, Result>? recovery)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
            return result;

        if (recovery is null)
        {
            return Result.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(Recover)));
        }

        var recovered = recovery(result.Errors);

        if (recovered is null)
        {
            return Result.Fail(
                ResultOutcome.Failure.Flow.ResultMissing(
                    nameof(Recover)));
        }

        return recovered.WithMetadataFrom(result);
    }

    public static Result<TValue> Recover<TValue>(
        this Result<TValue> result,
        Func<List<Error>, Result<TValue>>? recovery)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
            return result;

        if (recovery is null)
        {
            return Result<TValue>.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(Recover)));
        }

        var recovered = recovery(result.Errors);

        if (recovered is null)
        {
            return Result<TValue>.Fail(
                ResultOutcome.Failure.Flow.ResultMissing(
                    nameof(Recover)));
        }

        return recovered.WithMetadataFrom(result);
    }

    #endregion

    #region OrElse

    public static Result OrElse(
        this Result result,
        Func<Result>? fallback)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
            return result;

        if (fallback is null)
        {
            return Result.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(OrElse)));
        }

        var recovered = fallback();

        if (recovered is null)
        {
            return Result.Fail(
                ResultOutcome.Failure.Flow.ResultMissing(
                    nameof(OrElse)));
        }

        return recovered.WithMetadataFrom(result);
    }

    public static Result<TValue> OrElse<TValue>(
        this Result<TValue> result,
        Func<Result<TValue>>? fallback)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
            return result;

        if (fallback is null)
        {
            return Result<TValue>.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(OrElse)));
        }

        var recovered = fallback();

        if (recovered is null)
        {
            return Result<TValue>.Fail(
                ResultOutcome.Failure.Flow.ResultMissing(
                    nameof(OrElse)));
        }

        return recovered.WithMetadataFrom(result);
    }

    #endregion

    #region Try

    public static Result Try(
        Func<Result>? operation,
        Func<Exception, Error>? errorFactory = null)
    {
        if (operation is null)
        {
            return Result.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(Try)));
        }

        try
        {
            var result = operation();

            if (result is null)
            {
                return Result.Fail(
                    ResultOutcome.Failure.Flow.ResultMissing(
                        nameof(Try)));
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Result.Fail(
                CreateExceptionError(
                    exception,
                    errorFactory,
                    nameof(Try)));
        }
    }

    public static Result<TValue> Try<TValue>(
        Func<Result<TValue>>? operation,
        Func<Exception, Error>? errorFactory = null)
    {
        if (operation is null)
        {
            return Result<TValue>.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(Try)));
        }

        try
        {
            var result = operation();

            if (result is null)
            {
                return Result<TValue>.Fail(
                    ResultOutcome.Failure.Flow.ResultMissing(
                        nameof(Try)));
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Result<TValue>.Fail(
                CreateExceptionError(
                    exception,
                    errorFactory,
                    nameof(Try)));
        }
    }

    public static Result<TOut> MapTry<TValue, TOut>(
        this Result<TValue> result,
        Func<TValue, TOut>? mapper,
        Func<Exception, Error>? errorFactory = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
        {
            return Result<TOut>.Fail(
                result.Errors,
                result.StatusCode)
                .WithMetadataFrom(result);
        }

        if (mapper is null)
        {
            return Result<TOut>.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(MapTry)));
        }

        try
        {
            return Result<TOut>.Ok(
                    mapper(result.Value),
                    result.StatusCode)
                .WithMetadataFrom(result);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Result<TOut>.Fail(
                CreateExceptionError(
                    exception,
                    errorFactory,
                    nameof(MapTry)));
        }
    }

    public static Result<TOut> BindTry<TValue, TOut>(
        this Result<TValue> result,
        Func<TValue, Result<TOut>>? binder,
        Func<Exception, Error>? errorFactory = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
        {
            return Result<TOut>.Fail(
                result.Errors,
                result.StatusCode)
                .WithMetadataFrom(result);
        }

        if (binder is null)
        {
            return Result<TOut>.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(BindTry)));
        }

        try
        {
            var bound = binder(result.Value);

            if (bound is null)
            {
                return Result<TOut>.Fail(
                    ResultOutcome.Failure.Flow.ResultMissing(
                        nameof(BindTry)));
            }

            return bound.WithMetadataFrom(result);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Result<TOut>.Fail(
                CreateExceptionError(
                    exception,
                    errorFactory,
                    nameof(BindTry)));
        }
    }

    #endregion

    #region Effects

    public static Result Tap(
        this Result result,
        Action? action)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
            action?.Invoke();

        return result;
    }

    public static Result<TValue> Tap<TValue>(
        this Result<TValue> result,
        Action<TValue>? action)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
            action?.Invoke(result.Value);

        return result;
    }

    public static Result TapError(
        this Result result,
        Action<List<Error>>? action)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
            action?.Invoke(result.Errors);

        return result;
    }

    public static Result<TValue> TapError<TValue>(
        this Result<TValue> result,
        Action<List<Error>>? action)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
            action?.Invoke(result.Errors);

        return result;
    }

    public static Result Switch(
        this Result result,
        Action? onSuccess,
        Action<List<Error>>? onFailure)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
            onSuccess?.Invoke();
        else
            onFailure?.Invoke(result.Errors);

        return result;
    }

    public static Result<TValue> Switch<TValue>(
        this Result<TValue> result,
        Action<TValue>? onSuccess,
        Action<List<Error>>? onFailure)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
            onSuccess?.Invoke(result.Value);
        else
            onFailure?.Invoke(result.Errors);

        return result;
    }

    #endregion

    #region Combination

    public static Result Combine(
        params Result[] results)
    {
        ArgumentNullException.ThrowIfNull(results);

        if (results.Length == 0)
            return Result.Ok();

        foreach (var result in results)
        {
            if (result is null)
            {
                return Result.Fail(
                    ResultOutcome.Failure.Flow.ResultMissing(
                        nameof(Combine)));
            }
        }

        var failures = results
            .Where(result => result.IsFailure)
            .ToList();

        if (failures.Count == 0)
            return Result.Ok();

        var statusCode = failures[0].StatusCode;

        if (failures.Any(
                result => result.StatusCode != statusCode))
        {
            return Result.Fail(
                ResultOutcome.Failure.Errors.MixedStatusCodes);
        }

        var errors = failures
            .SelectMany(result => result.Errors)
            .ToList();

        return Result.Failure(
            errors,
            statusCode);
    }

    #endregion

    #region Extraction

    public static TValue? ValueOr<TValue>(
        this Result<TValue> result,
        TValue? fallback = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsSuccess
            ? result.Value
            : fallback;
    }

    public static TValue ValueOrThrow<TValue>(
        this Result<TValue> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        result.ThrowIfFailure();

        return result.Value;
    }

    public static Result ThrowIfFailure(
        this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
            throw new ResultException(result);

        return result;
    }

    public static Result<TValue> ThrowIfFailure<TValue>(
        this Result<TValue> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure)
            throw new ResultException(result);

        return result;
    }

    #endregion

    #region Exception Helpers

    private static Error CreateExceptionError(
        Exception exception,
        Func<Exception, Error>? errorFactory,
        string operation)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (errorFactory is null)
        {
            return Error.InternalServerError(
                "result.exception",
                "An unexpected exception occurred.");
        }

        var error = errorFactory(exception);

        return error
            ?? ResultOutcome.Failure.Flow.ExceptionMappingFailed(
                operation);
    }

    #endregion

    // =====================================================================
    // MatchAsync
    // =====================================================================

    /// <summary>
    /// Selects and executes the callback for the current result state.
    /// Legacy callback overload.
    /// </summary>
    public static Task<TResult?> MatchAsync<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, Task<TResult>>? onSuccess,
        Func<List<Error>, Task<TResult>>? onFailure,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return MatchAsyncCore<TValue, TResult>(
            result,
            onSuccess is null
                ? null
                : (value, _) => onSuccess(value),
            onFailure is null
                ? null
                : (errors, _) => onFailure(errors),
            cancellationToken);
    }

    /// <summary>
    /// Selects and executes the callback for the current result state.
    /// Token-aware callback overload.
    /// </summary>
    public static Task<TResult?> MatchAsync<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, CancellationToken, Task<TResult>>? onSuccess,
        Func<List<Error>, CancellationToken, Task<TResult>>? onFailure,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return MatchAsyncCore(
            result,
            onSuccess,
            onFailure,
            cancellationToken);
    }

    private static async Task<TResult?> MatchAsyncCore<TValue, TResult>(
        Result<TValue> result,
        Func<TValue, CancellationToken, Task<TResult>>? onSuccess,
        Func<List<Error>, CancellationToken, Task<TResult>>? onFailure,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsSuccess)
        {
            if (onSuccess is null)
            {
                return default;
            }

            var task = onSuccess(result.Value, cancellationToken);

            if (task is null)
            {
                return default;
            }

            var value = await task
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            return value;
        }

        if (onFailure is null)
        {
            return default;
        }

        var failureTask = onFailure(result.Errors, cancellationToken);

        if (failureTask is null)
        {
            return default;
        }

        var failureValue = await failureTask
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return failureValue;
    }

    // =====================================================================
    // FoldAsync
    // =====================================================================

    /// <summary>
    /// Folds the result into a single value.
    /// Legacy callback overload.
    /// </summary>
    public static Task<TResult?> FoldAsync<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, Task<TResult>>? onSuccess,
        Func<List<Error>, Task<TResult>>? onFailure,
        CancellationToken cancellationToken = default)
    {
        return MatchAsync(
            result,
            onSuccess,
            onFailure,
            cancellationToken);
    }

    /// <summary>
    /// Folds the result into a single value.
    /// Token-aware callback overload.
    /// </summary>
    public static Task<TResult?> FoldAsync<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, CancellationToken, Task<TResult>>? onSuccess,
        Func<List<Error>, CancellationToken, Task<TResult>>? onFailure,
        CancellationToken cancellationToken = default)
    {
        return MatchAsync(
            result,
            onSuccess,
            onFailure,
            cancellationToken);
    }

    // =====================================================================
    // MapAsync
    // =====================================================================

    /// <summary>
    /// Transforms a successful value while preserving failure.
    /// Legacy callback overload.
    /// </summary>
    public static Task<Result<TOut>> MapAsync<TValue, TOut>(
        this Result<TValue> result,
        Func<TValue, Task<TOut>>? mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return MapAsyncCore<TValue, TOut>(
            result,
            mapper is null
                ? null
                : (value, _) => mapper(value),
            cancellationToken);
    }

    /// <summary>
    /// Transforms a successful value while preserving failure.
    /// Token-aware callback overload.
    /// </summary>
    public static Task<Result<TOut>> MapAsync<TValue, TOut>(
        this Result<TValue> result,
        Func<TValue, CancellationToken, Task<TOut>>? mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return MapAsyncCore(result, mapper, cancellationToken);
    }

    private static async Task<Result<TOut>> MapAsyncCore<TValue, TOut>(
        Result<TValue> result,
        Func<TValue, CancellationToken, Task<TOut>>? mapper,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsFailure)
        {
            return Result<TOut>.Fail(result.Errors)
                .WithMetadataFrom(result);
        }

        if (mapper is null)
        {
            return Result<TOut>.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(MapAsync)));
        }

        var task = mapper(result.Value, cancellationToken);

        if (task is null)
        {
            return Result<TOut>.Fail(
                ResultOutcome.Failure.Flow.TaskMissing(
                    nameof(MapAsync)));
        }

        var value = await task
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return Result<TOut>.Ok(value)
            .WithMetadataFrom(result);
    }

    // =====================================================================
    // BindAsync
    // =====================================================================

    /// <summary>
    /// Chains an asynchronous operation returning another Result.
    /// Legacy callback overload.
    /// </summary>
    public static Task<Result<TOut>> BindAsync<TValue, TOut>(
        this Result<TValue> result,
        Func<TValue, Task<Result<TOut>>>? binder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return BindAsyncCore<TValue, TOut>(
            result,
            binder is null
                ? null
                : (value, _) => binder(value),
            cancellationToken);
    }

    /// <summary>
    /// Chains an asynchronous operation returning another Result.
    /// Token-aware callback overload.
    /// </summary>
    public static Task<Result<TOut>> BindAsync<TValue, TOut>(
        this Result<TValue> result,
        Func<TValue, CancellationToken, Task<Result<TOut>>>? binder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return BindAsyncCore(result, binder, cancellationToken);
    }

    private static async Task<Result<TOut>> BindAsyncCore<TValue, TOut>(
        Result<TValue> result,
        Func<TValue, CancellationToken, Task<Result<TOut>>>? binder,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsFailure)
        {
            return Result<TOut>.Fail(result.Errors)
                .WithMetadataFrom(result);
        }

        if (binder is null)
        {
            return Result<TOut>.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(BindAsync)));
        }

        var task = binder(result.Value, cancellationToken);

        if (task is null)
        {
            return Result<TOut>.Fail(
                ResultOutcome.Failure.Flow.TaskMissing(
                    nameof(BindAsync)));
        }

        var next = await task
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        if (next is null)
        {
            return Result<TOut>.Fail(
                ResultOutcome.Failure.Flow.ResultMissing(
                    nameof(BindAsync)));
        }

        return next.WithMetadataFrom(result);
    }

    // =====================================================================
    // EnsureAsync
    // =====================================================================

    /// <summary>
    /// Validates a successful value asynchronously.
    /// </summary>
    public static Task<Result<TValue>> EnsureAsync<TValue>(
        this Result<TValue> result,
        Func<TValue, Task<bool>>? predicate,
        Error error,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(error);

        return EnsureAsyncCore(
            result,
            predicate is null
                ? null
                : (value, _) => predicate(value),
            error,
            cancellationToken);
    }

    /// <summary>
    /// Validates a successful value asynchronously with cancellation.
    /// </summary>
    public static Task<Result<TValue>> EnsureAsync<TValue>(
        this Result<TValue> result,
        Func<TValue, CancellationToken, Task<bool>>? predicate,
        Error error,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(error);

        return EnsureAsyncCore(
            result,
            predicate,
            error,
            cancellationToken);
    }

    private static async Task<Result<TValue>> EnsureAsyncCore<TValue>(
        Result<TValue> result,
        Func<TValue, CancellationToken, Task<bool>>? predicate,
        Error error,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsFailure)
        {
            return result;
        }

        if (predicate is null)
        {
            return Result<TValue>.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(EnsureAsync)));
        }

        var task = predicate(result.Value, cancellationToken);

        if (task is null)
        {
            return Result<TValue>.Fail(
                ResultOutcome.Failure.Flow.TaskMissing(
                    nameof(EnsureAsync)));
        }

        var isValid = await task
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return isValid
            ? result
            : Result<TValue>.Fail(error)
                .WithMetadataFrom(result);
    }

    // =====================================================================
    // RecoverAsync
    // =====================================================================

    /// <summary>
    /// Recovers from failure by asynchronously producing a value.
    /// </summary>
    public static Task<Result<TValue>> RecoverAsync<TValue>(
        this Result<TValue> result,
        Func<List<Error>, Task<TValue>>? recovery,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return RecoverAsyncCore(
            result,
            recovery is null
                ? null
                : (errors, _) => recovery(errors),
            cancellationToken);
    }

    /// <summary>
    /// Recovers from failure using a cancellation-aware callback.
    /// </summary>
    public static Task<Result<TValue>> RecoverAsync<TValue>(
        this Result<TValue> result,
        Func<List<Error>, CancellationToken, Task<TValue>>? recovery,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return RecoverAsyncCore(result, recovery, cancellationToken);
    }

    private static async Task<Result<TValue>> RecoverAsyncCore<TValue>(
        Result<TValue> result,
        Func<List<Error>, CancellationToken, Task<TValue>>? recovery,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsSuccess)
        {
            return result;
        }

        if (recovery is null)
        {
            return Result<TValue>.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(RecoverAsync)));
        }

        var task = recovery(result.Errors, cancellationToken);

        if (task is null)
        {
            return Result<TValue>.Fail(
                ResultOutcome.Failure.Flow.TaskMissing(
                    nameof(RecoverAsync)));
        }

        var value = await task
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return Result<TValue>.Ok(value)
            .WithMetadataFrom(result);
    }

    // =====================================================================
    // OrElseAsync
    // =====================================================================

    /// <summary>
    /// Replaces a failure with an asynchronously produced Result.
    /// </summary>
    public static Task<Result<TValue>> OrElseAsync<TValue>(
        this Result<TValue> result,
        Func<List<Error>, Task<Result<TValue>>>? fallback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return OrElseAsyncCore(
            result,
            fallback is null
                ? null
                : (errors, _) => fallback(errors),
            cancellationToken);
    }

    /// <summary>
    /// Replaces a failure using a cancellation-aware callback.
    /// </summary>
    public static Task<Result<TValue>> OrElseAsync<TValue>(
        this Result<TValue> result,
        Func<List<Error>, CancellationToken, Task<Result<TValue>>>? fallback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return OrElseAsyncCore(result, fallback, cancellationToken);
    }

    private static async Task<Result<TValue>> OrElseAsyncCore<TValue>(
        Result<TValue> result,
        Func<List<Error>, CancellationToken, Task<Result<TValue>>>? fallback,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsSuccess)
        {
            return result;
        }

        if (fallback is null)
        {
            return Result<TValue>.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(OrElseAsync)));
        }

        var task = fallback(result.Errors, cancellationToken);

        if (task is null)
        {
            return Result<TValue>.Fail(
                ResultOutcome.Failure.Flow.TaskMissing(
                    nameof(OrElseAsync)));
        }

        var recovered = await task
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        if (recovered is null)
        {
            return Result<TValue>.Fail(
                ResultOutcome.Failure.Flow.ResultMissing(
                    nameof(OrElseAsync)));
        }

        return recovered.WithMetadataFrom(result);
    }

    // =====================================================================
    // TapAsync
    // =====================================================================

    /// <summary>
    /// Executes an optional asynchronous side effect on success.
    /// </summary>
    public static Task<Result<TValue>> TapAsync<TValue>(
        this Result<TValue> result,
        Func<TValue, Task>? action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return TapAsyncCore(
            result,
            action is null
                ? null
                : (value, _) => action(value),
            cancellationToken);
    }

    /// <summary>
    /// Executes a cancellation-aware side effect on success.
    /// </summary>
    public static Task<Result<TValue>> TapAsync<TValue>(
        this Result<TValue> result,
        Func<TValue, CancellationToken, Task>? action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return TapAsyncCore(result, action, cancellationToken);
    }

    private static async Task<Result<TValue>> TapAsyncCore<TValue>(
        Result<TValue> result,
        Func<TValue, CancellationToken, Task>? action,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsSuccess && action is not null)
        {
            var task = action(result.Value, cancellationToken);

            if (task is not null)
            {
                await task
                    .WaitAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        return result;
    }

    // =====================================================================
    // TapErrorAsync
    // =====================================================================

    /// <summary>
    /// Executes an optional asynchronous side effect on failure.
    /// </summary>
    public static Task<Result<TValue>> TapErrorAsync<TValue>(
        this Result<TValue> result,
        Func<List<Error>, Task>? action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return TapErrorAsyncCore(
            result,
            action is null
                ? null
                : (errors, _) => action(errors),
            cancellationToken);
    }

    /// <summary>
    /// Executes a cancellation-aware side effect on failure.
    /// </summary>
    public static Task<Result<TValue>> TapErrorAsync<TValue>(
        this Result<TValue> result,
        Func<List<Error>, CancellationToken, Task>? action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return TapErrorAsyncCore(result, action, cancellationToken);
    }

    private static async Task<Result<TValue>> TapErrorAsyncCore<TValue>(
        Result<TValue> result,
        Func<List<Error>, CancellationToken, Task>? action,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsFailure && action is not null)
        {
            var task = action(result.Errors, cancellationToken);

            if (task is not null)
            {
                await task
                    .WaitAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        return result;
    }

    // =====================================================================
    // SwitchAsync
    // =====================================================================

    /// <summary>
    /// Executes an optional callback for the current result state.
    /// </summary>
    public static Task SwitchAsync(
        this Result result,
        Func<Task>? onSuccess,
        Func<List<Error>, Task>? onFailure,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return SwitchAsyncCore(
            result,
            onSuccess is null
                ? null
                : _ => onSuccess(),
            onFailure is null
                ? null
                : (errors, _) => onFailure(errors),
            cancellationToken);
    }

    /// <summary>
    /// Executes a cancellation-aware callback for the current result state.
    /// </summary>
    public static Task SwitchAsync(
        this Result result,
        Func<CancellationToken, Task>? onSuccess,
        Func<List<Error>, CancellationToken, Task>? onFailure,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return SwitchAsyncCore(
            result,
            onSuccess,
            onFailure,
            cancellationToken);
    }

    private static async Task SwitchAsyncCore(
        Result result,
        Func<CancellationToken, Task>? onSuccess,
        Func<List<Error>, CancellationToken, Task>? onFailure,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var task = result.IsSuccess
            ? onSuccess?.Invoke(cancellationToken)
            : onFailure?.Invoke(result.Errors, cancellationToken);

        // Both callbacks are optional side effects.
        if (task is null)
        {
            return;
        }

        await task
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();
    }

    // =====================================================================
    // TryAsync<TValue>
    // =====================================================================

    /// <summary>
    /// Executes an operation and converts ordinary exceptions to failures.
    /// OperationCanceledException is always propagated.
    /// </summary>
    public static Task<Result<TValue>> TryAsync<TValue>(
        this Func<Task<TValue>>? operation,
        Func<Exception, Error>? errorFactory = null,
        CancellationToken cancellationToken = default)
    {
        return TryAsyncCore<TValue>(
            operation is null
                ? null
                : _ => operation(),
            errorFactory,
            cancellationToken);
    }

    /// <summary>
    /// Executes a cancellation-aware operation and converts ordinary
    /// exceptions to failures.
    /// </summary>
    public static Task<Result<TValue>> TryAsync<TValue>(
        this Func<CancellationToken, Task<TValue>>? operation,
        Func<Exception, Error>? errorFactory = null,
        CancellationToken cancellationToken = default)
    {
        return TryAsyncCore(operation, errorFactory, cancellationToken);
    }

    private static async Task<Result<TValue>> TryAsyncCore<TValue>(
        Func<CancellationToken, Task<TValue>>? operation,
        Func<Exception, Error>? errorFactory,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (operation is null)
        {
            return Result<TValue>.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(TryAsync)));
        }

        try
        {
            var task = operation(cancellationToken);

            if (task is null)
            {
                return Result<TValue>.Fail(
                    ResultOutcome.Failure.Flow.TaskMissing(
                        nameof(TryAsync)));
            }

            var value = await task
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<TValue>.Ok(value);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Result<TValue>.Fail(
                CreateExceptionError(
                    exception,
                    errorFactory,
                    nameof(TryAsync)));
        }
    }

    // =====================================================================
    // TryAsync (non-generic)
    // =====================================================================

    /// <summary>
    /// Executes a Task-returning operation and converts ordinary exceptions
    /// to failures. Cancellation is propagated.
    /// </summary>
    public static Task<Result> TryAsync(
        this Func<Task>? operation,
        Func<Exception, Error>? errorFactory = null,
        CancellationToken cancellationToken = default)
    {
        return TryAsyncCore(
            operation is null
                ? null
                : async _ =>
                {
                    await operation().ConfigureAwait(false);
                },
            errorFactory,
            cancellationToken);
    }

    /// <summary>
    /// Executes a cancellation-aware Task-returning operation.
    /// </summary>
    public static Task<Result> TryAsync(
        this Func<CancellationToken, Task>? operation,
        Func<Exception, Error>? errorFactory = null,
        CancellationToken cancellationToken = default)
    {
        return TryAsyncCore(operation, errorFactory, cancellationToken);
    }

    private static async Task<Result> TryAsyncCore(
        Func<CancellationToken, Task>? operation,
        Func<Exception, Error>? errorFactory,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (operation is null)
        {
            return Result.Fail(
                ResultOutcome.Failure.Flow.CallbackMissing(
                    nameof(TryAsync)));
        }

        try
        {
            var task = operation(cancellationToken);

            if (task is null)
            {
                return Result.Fail(
                    ResultOutcome.Failure.Flow.TaskMissing(
                        nameof(TryAsync)));
            }

            await task
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            return Result.Ok();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Result.Fail(
                CreateExceptionError(
                    exception,
                    errorFactory,
                    nameof(TryAsync)));
        }
    }
}