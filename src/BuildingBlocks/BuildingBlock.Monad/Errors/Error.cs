using BuildingBlock.Monad.Metadata;

namespace BuildingBlock.Monad.Errors;

/// <summary>
/// Represents a domain or infrastructure error with a machine-readable code,
/// human-readable message, and optional Problem Details information.
/// </summary>
/// <remarks>
/// <para>
/// The primary error identity is <see cref="Code"/> and <see cref="Message"/>.
/// </para>
/// <para>
/// <see cref="Type"/>, <see cref="Instance"/>, and <see cref="Status"/> provide
/// optional Problem Details information. <see cref="Severity"/> describes the
/// operational importance of the error.
/// </para>
/// </remarks>
public partial record Error : IError
{
    #region Properties

    /// <summary>
    /// Gets the machine-readable error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the human-readable error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the optional error type category.
    /// </summary>
    public string? Type { get; }

    /// <summary>
    /// Gets the optional unique instance identifier.
    /// </summary>
    public string? Instance { get; }

    /// <summary>
    /// Gets the optional HTTP status code.
    /// </summary>
    public int? Status { get; }

    /// <summary>
    /// Gets the severity level of the error.
    /// </summary>
    public ErrorSeverity Severity { get; }

    /// <summary>
    /// Gets the metadata attached to the error.
    /// </summary>
    public MetadataDictionary Metadata { get;  set; } = new();

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new error.
    /// </summary>
    /// <param name="code">The machine-readable error code.</param>
    /// <param name="message">The human-readable error message.</param>
    /// <param name="status">The optional HTTP status code.</param>
    /// <param name="type">The optional error type category.</param>
    /// <param name="instance">The optional unique instance identifier.</param>
    /// <param name="severity">The error severity.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="code"/> or <paramref name="message"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="code"/> or <paramref name="message"/> is whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="code"/>, <paramref name="message"/>, or
    /// <paramref name="status"/> violates its constraint.
    /// </exception>
    public Error(
        string code,
        string message,
        int? status = null,
        string? type = null,
        string? instance = null,
        ErrorSeverity severity = ErrorSeverity.Error)
    {
        ErrorGuard.ValidateCode(code);
        ErrorGuard.ValidateMessage(message);

        if (status.HasValue)
            ErrorGuard.ValidateStatus(status.Value);

        ErrorGuard.ValidateType(type);
        ErrorGuard.ValidateInstance(instance);

        Code = code;
        Message = message;
        Type = type;
        Instance = instance;
        Status = status;
        Severity = severity;
    }

    #endregion

    #region Equality

    /// <summary>
    /// Compares this error with another error using the error value members.
    /// </summary>
    /// <param name="other">The error to compare against.</param>
    /// <returns>
    /// <c>true</c> when the error value members are equal; otherwise <c>false</c>.
    /// </returns>
    public bool Equals(IError? other)
        => other is not null
        && Code == other.Code
        && Message == other.Message
        && Status == other.Status
        && Type == other.Type
        && Instance == other.Instance
        && Severity == other.Severity;

    /// <summary>
    /// Computes the hash code for this error.
    /// </summary>
    /// <returns>The combined hash code of the error value members.</returns>
    public override int GetHashCode()
        => HashCode.Combine(
            Code,
            Message,
            Status,
            Type,
            Instance,
            Severity);

    #endregion

    #region Formatting

    /// <summary>
    /// Returns the canonical <c>Code:Message</c> representation.
    /// </summary>
    /// <returns>The error code and message.</returns>
    public override string ToString()
        => $"{Code}:{Message}";

    #endregion
}
