using BuildingBlock.Monad.Metadata;

namespace BuildingBlock.Monad.Errors;

public interface IError : IHasMetadata<MetadataDictionary>
{
    string Code { get; }
    string Message { get; }
    string? Type { get; }
    string? Instance { get; }
    int? Status { get; }
    ErrorSeverity Severity { get; }
}