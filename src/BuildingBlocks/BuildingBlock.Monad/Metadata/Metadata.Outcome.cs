namespace BuildingBlock.Monad.Metadata;

public static class MetadataOutcome
{
    public static class Success
    {
        // TODO: expand this in the future
    }
    public static class Failure
    {
        public static class Request
        {
            public static class Argument
            {
                public static class Null
                {
                    public const string Code = "request.argument.null";
                    public const string Message = "The request argument must not be null.";
                    public const string Representation = Code + ":" + Message;
                }
            }
        }

        public static class Key
        {
            public static class Argument
            {
                public static class Null
                {
                    public const string Code = "key.argument.null";
                    public const string Message = "The key argument must not be null.";
                    public const string Representation = Code + ":" + Message;
                }
            }

            public static class Metadata
            {
                public static class NotFound
                {
                    public const string Code = "key.metadata.not_found";
                    public const string Message = "The specified metadata key was not found.";
                    public const string Representation = Code + ":" + Message;
                }
            }
        }

        public static class Value
        {
            public static class Metadata
            {
                public static class InvalidType
                {
                    public const string Code = "value.metadata.invalid_type";
                    public const string Message = "The metadata value is not of the requested type.";
                    public const string Representation = Code + ":" + Message;

                }
            }
        }

        public static class Dictionary
        {
            public static class Argument
            {
                public static class Null
                {
                    public const string Code = "dictionary.argument.null";
                    public const string Message = "The source dictionary must not be null.";
                    public const string Representation = Code + ":" + Message;
                }
            }

            public static class Mutation
            {
                public static class NotSupported
                {
                    public const string Code = "dictionary.mutation.not_supported";
                    public const string Message =
                        "The underlying metadata dictionary does not support mutation.";

                    public const string Representation = Code + ":" + Message;
                }
            }
        }
    }
}