using BuildingBlock.Monad.Errors;

namespace BuildingBlock.Monad.Results;

public static class ResultConstant
{
    #region Constraints

    public static class Constraint
    {
        public static class Errors
        {
            public const int MaxCount = 50;
        }

        public static class Metadata
        {
            public const int MaxEntries = 50;
        }

        public static class Status
        {
            public const int Min = 100;

            public const int Max = 599;

            public static class Success
            {
                public const int Min = 200;
                public const int Max = 299;
            }

            public static class Failure
            {
                public const int Min = 400;

                public const int Max = 599;
            }
        }
    }

    #endregion

    #region Defaults

    public static class Default
    {
        public const int FailureStatus = StatusCode.InternalServerError;

        public static readonly List<Error> EmptyErrors = [];
    }

    #endregion

    #region Status Codes

    public static class StatusCode
    {
        #region Success

        public const int Ok = 200;
        public const int Created = 201;
        public const int Accepted = 202;
        public const int NoContent = 204;

        #endregion

        #region Failure

        public const int BadRequest = ErrorConstant.StatusCode.BadRequest;
        public const int Unauthorized = ErrorConstant.StatusCode.Unauthorized;
        public const int Forbidden = ErrorConstant.StatusCode.Forbidden;
        public const int NotFound = ErrorConstant.StatusCode.NotFound;
        public const int Conflict = ErrorConstant.StatusCode.Conflict;
        public const int UnprocessableEntity =
            ErrorConstant.StatusCode.UnprocessableEntity;

        public const int InternalServerError =
            ErrorConstant.StatusCode.InternalServerError;

        public const int ServiceUnavailable =
            ErrorConstant.StatusCode.ServiceUnavailable;

        #endregion
    }

    #endregion
}