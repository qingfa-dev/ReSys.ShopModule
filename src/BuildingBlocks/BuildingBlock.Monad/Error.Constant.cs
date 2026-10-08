namespace BuildingBlock.Monad;

public static class ErrorConstant
{
    // Constraint:
    public static class Constraint
    {
        public static class Code
        {
            public const int MaxLength = 256;
        }

        public static class Message
        {
            public const int MaxLength = 1024;
        }
    }
    // Default:
    // Enumerate:
    // Failure:
}