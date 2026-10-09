namespace BuildingBlock.Core.Domain.Concerns.Foundation.Identifiable;

/// <summary>Base concern for entity identification.</summary>
/// <typeparam name="TKey">The type of the identifier.</typeparam>
public interface IIdentifiable<TKey>
{
    /// <summary>Gets the unique identifier for the entity.</summary>
    TKey Id { get; }
}