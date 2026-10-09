using BuildingBlock.Monad.Errors;

namespace BuildingBlock.Monad.Results;

public static class ResultGuard
{
    #region Status

    public static Error? ValidateStatusCode(
        int statusCode,
        bool isSuccess)
    {
        if (statusCode is
            < ResultConstant.Constraint.Status.Min or
            > ResultConstant.Constraint.Status.Max)
        {
            return ResultOutcome.Failure.Status.InvalidStatusCode(statusCode);
        }

        if (isSuccess)
        {
            return statusCode is
                < ResultConstant.Constraint.Status.Success.Min or
                > ResultConstant.Constraint.Status.Success.Max
                    ? ResultOutcome.Failure.Status.InvalidSuccessStatus(statusCode)
                    : null;
        }

        return statusCode is
            < ResultConstant.Constraint.Status.Failure.Min or
            > ResultConstant.Constraint.Status.Failure.Max
                ? ResultOutcome.Failure.Status.InvalidFailureStatus(statusCode)
                : null;
    }

    #endregion

    #region Errors

    public static Error? ValidateErrors(
        List<Error> errors,
        bool isSuccess)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Count > ResultConstant.Constraint.Errors.MaxCount)
        {
            return ResultOutcome.Failure.Errors.ExceedsMaxCount(
                ResultConstant.Constraint.Errors.MaxCount);
        }

        if (!isSuccess && errors.Count == 0)
        {
            return ResultOutcome.Failure.Errors.Empty;
        }

        return null;
    }

    #endregion

    #region Value

    public static Error? ValidateValue<TValue>(
        bool isSuccess,
        TValue? value)
    {
        if (isSuccess && value is null)
        {
            return ResultOutcome.Failure.Value.Required;
        }

        return null;
    }

    #endregion

    #region Result

    public static Error? ValidateResultConsistency(
        bool isSuccess,
        List<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (isSuccess || errors.Count <= 1)
        {
            return null;
        }

        var status = errors[0].Status;

        for (var index = 1; index < errors.Count; index++)
        {
            if (errors[index].Status != status)
            {
                return ResultOutcome.Failure.Errors.MixedStatusCodes;
            }
        }

        return null;
    }

    #endregion
}