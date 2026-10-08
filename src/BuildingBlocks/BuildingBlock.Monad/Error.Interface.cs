namespace BuildingBlock.Monad;

public interface IError
{
    int Code { get; }
    string Message { get; }
}