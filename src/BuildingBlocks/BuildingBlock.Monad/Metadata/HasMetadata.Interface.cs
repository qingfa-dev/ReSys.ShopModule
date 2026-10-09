namespace BuildingBlock.Monad.Metadata;

public interface IHasMetadata<TMetadata>
    where TMetadata : IMetadata
{
    TMetadata Metadata { get; }
}

public interface IHasMetadata : IHasMetadata<MetadataDictionary>
{
}