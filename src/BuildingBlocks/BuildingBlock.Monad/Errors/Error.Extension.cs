using BuildingBlock.Monad.Metadata;

namespace BuildingBlock.Monad.Errors;

/// <summary>
/// Provides factory, copy, metadata, and convenience operations for <see cref="Error"/>.
/// </summary>
public partial record Error
{
    #region Factory

    /// <summary>
    /// Creates a custom <see cref="Error"/> with full control over all members.
    /// </summary>
    /// <param name="code">The machine-readable error code.</param>
    /// <param name="message">The human-readable error message.</param>
    /// <param name="status">Optional HTTP status code.</param>
    /// <param name="type">Optional error type.</param>
    /// <param name="instance">Optional unique instance identifier.</param>
    /// <param name="severity">
    /// The error severity; defaults to <see cref="ErrorSeverity.Error"/>.
    /// </param>
    /// <returns>A new <see cref="Error"/> instance.</returns>
    public static Error Custom(
        string code,
        string message,
        int? status = null,
        string? type = null,
        string? instance = null,
        ErrorSeverity severity = ErrorSeverity.Error)
        => new(
            code,
            message,
            status,
            type,
            instance,
            severity);

    #endregion

    #region 4xx — Client Errors

     /// <summary>
     /// Creates a 400 Bad Request error.
     /// </summary>
     /// <param name="code">The machine-readable error code.</param>
     /// <param name="message">The human-readable error message.</param>
     /// <returns>A new <see cref="Error"/> with status 400.</returns>
     public static Error BadRequest(
         string code,
         string message)
         => Create(
             code,
             message,
             ErrorConstant.StatusCode.BadRequest,
             ErrorSeverity.Warning,
             "https://errors.kernel/bad-request");

     /// <summary>
     /// Creates a 401 Unauthorized error.
     /// </summary>
     /// <param name="code">The machine-readable error code.</param>
     /// <param name="message">The human-readable error message.</param>
     /// <returns>A new <see cref="Error"/> with status 401.</returns>
     public static Error Unauthorized(
         string code,
         string message)
         => Create(
             code,
             message,
             ErrorConstant.StatusCode.Unauthorized,
             ErrorSeverity.Error,
             "https://errors.kernel/unauthorized");

     /// <summary>
     /// Creates a 403 Forbidden error.
     /// </summary>
     /// <param name="code">The machine-readable error code.</param>
     /// <param name="message">The human-readable error message.</param>
     /// <returns>A new <see cref="Error"/> with status 403.</returns>
     public static Error Forbidden(
         string code,
         string message)
         => Create(
             code,
             message,
             ErrorConstant.StatusCode.Forbidden,
             ErrorSeverity.Error,
             "https://errors.kernel/forbidden");

     /// <summary>
     /// Creates a 404 Not Found error.
     /// </summary>
     /// <param name="code">The machine-readable error code.</param>
     /// <param name="message">The human-readable error message.</param>
     /// <returns>A new <see cref="Error"/> with status 404.</returns>
     public static Error NotFound(
         string code,
         string message)
         => Create(
             code,
             message,
             ErrorConstant.StatusCode.NotFound,
             ErrorSeverity.Warning,
             "https://errors.kernel/not-found");

     /// <summary>
     /// Creates a 409 Conflict error.
     /// </summary>
     /// <param name="code">The machine-readable error code.</param>
     /// <param name="message">The human-readable error message.</param>
     /// <returns>A new <see cref="Error"/> with status 409.</returns>
     public static Error Conflict(
         string code,
         string message)
         => Create(
             code,
             message,
             ErrorConstant.StatusCode.Conflict,
             ErrorSeverity.Warning,
             "https://errors.kernel/conflict");

     /// <summary>
     /// Creates a 422 Unprocessable Entity error.
     /// </summary>
     /// <param name="code">The machine-readable error code.</param>
     /// <param name="message">The human-readable error message.</param>
     /// <returns>A new <see cref="Error"/> with status 422.</returns>
     public static Error UnprocessableEntity(
         string code,
         string message)
         => Create(
             code,
             message,
             ErrorConstant.StatusCode.UnprocessableEntity,
             ErrorSeverity.Warning,
             "https://errors.kernel/unprocessable-entity");

    #endregion

    #region 5xx — Server Errors

     /// <summary>
     /// Creates a 500 Internal Server Error.
     /// </summary>
     /// <param name="code">The machine-readable error code.</param>
     /// <param name="message">The human-readable error message.</param>
     /// <returns>A new <see cref="Error"/> with status 500.</returns>
     public static Error InternalServerError(
         string code,
         string message)
         => Create(
             code,
             message,
             ErrorConstant.StatusCode.InternalServerError,
             ErrorSeverity.Critical,
             "https://errors.kernel/internal-server-error");

     /// <summary>
     /// Creates a 503 Service Unavailable error.
     /// </summary>
     /// <param name="code">The machine-readable error code.</param>
     /// <param name="message">The human-readable error message.</param>
     /// <returns>A new <see cref="Error"/> with status 503.</returns>
     public static Error ServiceUnavailable(
         string code,
         string message)
         => Create(
             code,
             message,
             ErrorConstant.StatusCode.ServiceUnavailable,
             ErrorSeverity.Critical,
             "https://errors.kernel/service-unavailable");

    #endregion

    #region Private Factory

    private static Error Create(
        string code,
        string message,
        int status,
        ErrorSeverity severity = ErrorSeverity.Error,
        string? type = null)
        => new(
            code: code,
            message: message,
            status: status,
            type: type,
            severity: severity);

    #endregion
    #region Copy

    /// <summary>
    /// Creates a copy of this error with the specified members overridden.
    /// </summary>
    /// <param name="code">
    /// The replacement code; the current value is retained when <c>null</c>.
    /// </param>
    /// <param name="message">
    /// The replacement message; the current value is retained when <c>null</c>.
    /// </param>
    /// <param name="status">
    /// The replacement status; the current value is retained when <c>null</c>.
    /// </param>
    /// <param name="type">
    /// The replacement type; the current value is retained when <c>null</c>.
    /// </param>
    /// <param name="instance">
    /// The replacement instance; the current value is retained when <c>null</c>.
    /// </param>
    /// <param name="severity">
    /// The replacement severity; the current value is retained when <c>null</c>.
    /// </param>
    /// <param name="metadata">
    /// The replacement metadata; the current value is retained when <c>null</c>.
    /// </param>
    /// <returns>A new error containing the requested overrides.</returns>
    public Error CopyWith(
        string? code = null,
        string? message = null,
        int? status = null,
        string? type = null,
        string? instance = null,
        ErrorSeverity? severity = null,
        MetadataDictionary? metadata = null)
    {
        return new Error(
            code ?? Code,
            message ?? Message,
            status ?? Status,
            type ?? Type,
            instance ?? Instance,
            severity ?? Severity)
        {
            Metadata = metadata ?? Metadata
        };
    }

    #endregion

    #region Metadata

    /// <summary>
    /// Adds or replaces a metadata entry.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>This error.</returns>
    public Error WithMetadata(string key, object value)
    {
        this.SetValue(key, value);
        return this;
    }

    /// <summary>
    /// Replaces the metadata dictionary.
    /// </summary>
    /// <param name="metadata">The replacement metadata.</param>
    /// <returns>This error.</returns>
     public Error WithMetadata(MetadataDictionary? metadata)
     {
         Metadata = metadata ?? new MetadataDictionary();
         return this;
     }

    /// <summary>
    /// Attaches a trace identifier.
    /// </summary>
    /// <param name="traceId">The trace identifier.</param>
    /// <returns>This error.</returns>
    public Error WithTraceId(string traceId)
        => WithMetadata(ErrorConstant.Metadata.TraceId, traceId);

    /// <summary>
    /// Attaches a timestamp.
    /// </summary>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns>This error.</returns>
    public Error WithTimestamp(DateTimeOffset timestamp)
        => WithMetadata(ErrorConstant.Metadata.Timestamp, timestamp);

    /// <summary>
    /// Attaches a resource identifier.
    /// </summary>
    /// <param name="resource">The resource identifier.</param>
    /// <returns>This error.</returns>
    public Error WithResource(string resource)
        => WithMetadata(ErrorConstant.Metadata.Resource, resource);

    /// <summary>
    /// Attaches a field identifier.
    /// </summary>
    /// <param name="field">The field identifier.</param>
    /// <returns>This error.</returns>
    public Error WithField(string field)
        => WithMetadata(ErrorConstant.Metadata.Field, field);

    /// <summary>
    /// Attaches an attempt number.
    /// </summary>
    /// <param name="attempt">The attempt number.</param>
    /// <returns>This error.</returns>
    public Error WithAttempt(int attempt)
        => WithMetadata(ErrorConstant.Metadata.Attempt, attempt);

    #endregion

    #region Value Overrides

    /// <summary>
    /// Creates a copy with a different error code.
    /// </summary>
    public Error WithCode(string code)
        => CopyWith(code: code);

    /// <summary>
    /// Creates a copy with a different error message.
    /// </summary>
    public Error WithMessage(string message)
        => CopyWith(message: message);

    /// <summary>
    /// Creates a copy with a different error type.
    /// </summary>
    public Error WithType(string type)
        => CopyWith(type: type);

    /// <summary>
    /// Creates a copy with a different instance identifier.
    /// </summary>
    public Error WithInstance(string instance)
        => CopyWith(instance: instance);

    /// <summary>
    /// Creates a copy with a different HTTP status.
    /// </summary>
    public Error WithStatus(int status)
        => CopyWith(status: status);

    /// <summary>
    /// Creates a copy with a different severity.
    /// </summary>
    public Error WithSeverity(ErrorSeverity severity)
        => CopyWith(severity: severity);

    #endregion

    #region Exception

    /// <summary>
    /// Creates an error from an exception.
    /// </summary>
    /// <param name="exception">The caught exception.</param>
    /// <param name="code">The error code.</param>
    /// <param name="severity">The error severity.</param>
    /// <returns>An error representing the exception.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exception"/> is <c>null</c>.
    /// </exception>
    public static Error FromException(
        Exception exception,
        string code = "general.error",
        ErrorSeverity severity = ErrorSeverity.Critical)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return InternalServerError(code, exception.Message)
            .WithSeverity(severity)
            .WithMetadata(
                "exception.type",
                exception.GetType().FullName!);
    }

    #endregion
}