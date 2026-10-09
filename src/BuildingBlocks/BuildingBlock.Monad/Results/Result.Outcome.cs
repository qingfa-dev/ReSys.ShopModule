using BuildingBlock.Monad.Errors;

namespace BuildingBlock.Monad.Results;

public static class ResultOutcome
{
    #region Failure

    public static class Failure
    {
        #region Errors

        public static class Errors
        {
            public static Error Empty =>
                Error.InternalServerError(
                    "result.errors.empty",
                    "At least one error is required.");

            public static Error ExceedsMaxCount(int maxCount) =>
                Error.InternalServerError(
                    "result.errors.exceeds_max_count",
                    $"A result cannot contain more than {maxCount} errors.");

            public static Error MixedStatusCodes =>
                Error.InternalServerError(
                    "result.errors.mixed_status_codes",
                    "All errors in a result must have the same HTTP status code.");
        }

        #endregion

        #region Metadata

        public static class Metadata
        {
            public static Error EmptyKey =>
                Error.InternalServerError(
                    "result.metadata.empty_key",
                    "Metadata keys cannot be null, empty, or whitespace.");

            public static Error NullValue(string key) =>
                Error.InternalServerError(
                    "result.metadata.null_value",
                    $"Metadata value for key '{key}' cannot be null.");

            public static Error ExceedsMaxEntries(int maxEntries) =>
                Error.InternalServerError(
                    "result.metadata.exceeds_max_entries",
                    $"Metadata cannot contain more than {maxEntries} entries.");
        }

        #endregion

        #region Value

        public static class Value
        {
            public static Error Required =>
                Error.InternalServerError(
                    "result.value.required",
                    "A successful result must have a value.");

            public static Error NotAllowed =>
                Error.InternalServerError(
                    "result.value.not_allowed",
                    "A failed result cannot have a value.");
        }

        #endregion

        #region Status

        public static class Status
        {
            public static Error InvalidStatusCode(int statusCode) =>
                Error.InternalServerError(
                    "result.status.invalid_status_code",
                    $"Status code must be between " +
                    $"{ResultConstant.Constraint.Status.Min} and " +
                    $"{ResultConstant.Constraint.Status.Max}, " +
                    $"but was {statusCode}.");

            public static Error InvalidSuccessStatus(int statusCode) =>
                Error.InternalServerError(
                    "result.status.invalid_success_status",
                    $"A successful result must have a status code between " +
                    $"{ResultConstant.Constraint.Status.Success.Min} and " +
                    $"{ResultConstant.Constraint.Status.Success.Max}, " +
                    $"but was {statusCode}.");

            public static Error InvalidFailureStatus(int statusCode) =>
                Error.InternalServerError(
                    "result.status.invalid_failure_status",
                    $"A failed result must have a status code between " +
                    $"{ResultConstant.Constraint.Status.Failure.Min} and " +
                    $"{ResultConstant.Constraint.Status.Failure.Max}, " +
                    $"but was {statusCode}.");
        }

        #endregion

        #region Flow

        public static class Flow
        {
            public static Error CallbackMissing(
                string operation)
                => Error.InternalServerError(
                    "result.flow.callback_missing",
                    $"The required callback for '{operation}' was not provided.");

            public static Error TaskMissing(
                string operation)
                => Error.InternalServerError(
                    "result.flow.task_missing",
                    $"The operation '{operation}' returned a null task.");

            public static Error ResultMissing(
                string operation)
                => Error.InternalServerError(
                    "result.flow.result_missing",
                    $"The operation '{operation}' returned a null result.");

            public static Error ErrorMissing(
                string operation)
                => Error.InternalServerError(
                    "result.flow.error_missing",
                    $"The operation '{operation}' returned a null error.");

            public static Error ExceptionMappingMissing(
                string operation)
                => Error.InternalServerError(
                    "result.flow.exception_mapping_missing",
                    $"The exception mapping for '{operation}' was not provided.");

            public static Error ExceptionMappingFailed(
                string operation)
                => Error.InternalServerError(
                    "result.flow.exception_mapping_failed",
                    $"The exception mapping for '{operation}' returned a null error.");
        }

        #endregion
    }

    #endregion
}