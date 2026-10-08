namespace BuildingBlock.Monad.Errors;

public static class ErrorConstant
{
    #region Constraints

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

        public static class Type
        {
            public const int MaxLength = 256;
        }

        public static class Status
        {
            public const int Min = 100;
            public const int Max = 599;
        }

        public static class Instance
        {
            public const int MaxLength = 256;
        }
    }

    #endregion

    #region Metadata

    public static class Metadata
    {
        public const string TraceId = nameof(TraceId); 
        public const string Timestamp = nameof(Timestamp); 
        public const string Resource = nameof(Resource); 
        public const string Field = nameof(Field); 
        public const string Attempt = nameof(Attempt);
    }

    #endregion

    #region Status Codes

    public static class StatusCode
    {
        #region Success

        public const int Ok = 200;
        public const int Created = 201;

        #endregion

        #region Failure

        public const int BadRequest = 400;
        public const int Unauthorized = 401;
        public const int Forbidden = 403;
        public const int NotFound = 404;
        public const int Conflict = 409;
        public const int UnprocessableEntity = 422;

        public const int InternalServerError = 500;
        public const int NotImplemented = 501;
        public const int BadGateway = 502;
        public const int ServiceUnavailable = 503;
        public const int GatewayTimeout = 504;

        #endregion
    }

    #region Result

    public static class Result
    {
        public static class Failure
        {
            public static class Status
            {
                public static class OutOfRange
                {
                    public const string Code =
                        "error.status.out_of_range";

                    public const string Message =
                        "The status code is out of the valid range ({0} - {1}).";

                    public const string Representation =
                        Code + ":" + Message;
                }
            }

            public static class Code
            {
                public static class NullOrWhitespace
                {
                    public const string Code =
                        "error.code.null_or_whitespace";

                    public const string Message =
                        "The error code cannot be null or whitespace.";

                    public const string Representation =
                        Code + ":" + Message;
                }

                public static class ExceedsMaxLength
                {
                    public const string Code =
                        "error.code.exceeds_max_length";

                    public const string Message =
                        "The error code exceeds the maximum length of '{0}' characters.";

                    public const string Representation =
                        Code + ":" + Message;
                }
            }

            public static class Message
            {
                public static class NullOrWhitespace
                {
                    public const string Code =
                        "error.message.null_or_whitespace";

                    public const string Message =
                        "The error message cannot be null or whitespace.";

                    public const string Representation =
                        Code + ":" + Message;
                }

                public static class ExceedsMaxLength
                {
                    public const string Code =
                        "error.message.exceeds_max_length";

                    public const string Message =
                        "The error message exceeds the maximum length of '{0}' characters.";

                    public const string Representation =
                        Code + ":" + Message;
                }
            }

            public static class Type
            {
                public static class ExceedsMaxLength
                {
                    public const string Code =
                        "error.type.exceeds_max_length";

                    public const string Message =
                        "The error type exceeds the maximum length of '{0}' characters.";

                    public const string Representation =
                        Code + ":" + Message;
                }
            }

            public static class Instance
            {
                public static class ExceedsMaxLength
                {
                    public const string Code =
                        "error.instance.exceeds_max_length";

                    public const string Message =
                        "The instance exceeds the maximum length of '{0}' characters.";

                    public const string Representation =
                        Code + ":" + Message;
                }
            }
        }
    }

    #endregion
    #endregion
}