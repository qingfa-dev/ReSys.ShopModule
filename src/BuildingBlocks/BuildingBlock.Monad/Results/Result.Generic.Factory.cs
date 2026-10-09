using BuildingBlock.Monad.Errors;

namespace BuildingBlock.Monad.Results;

public partial record Result<TValue>
{
    #region Factory
    #region Custom

    public static Result<TValue> Custom(
        bool isSuccess,
        TValue value,
        List<Error>? errors = null,
        int? statusCode = null)
        => new(
            isSuccess: isSuccess,
            value: value,
            errors: errors?.ToList(),
            statusCode: statusCode);

    #endregion
    #region Success

    public static Result<TValue> Ok(
        TValue value,
        int statusCode = ResultConstant.StatusCode.Ok)
        => new(
            isSuccess: true,
            value: value,
            statusCode: statusCode);

    public static Result<TValue> Success(
        TValue value,
        int statusCode = ResultConstant.StatusCode.Ok)
        => Ok(value, statusCode);

    public static Result<TValue> Created(
        TValue value)
        => Ok(
            value,
            ResultConstant.StatusCode.Created);

    public static Result<TValue> Accepted(
        TValue value)
        => Ok(
            value,
            ResultConstant.StatusCode.Accepted);

    #endregion

    #region Failure

    public static new Result<TValue> Failure(
        List<Error> errors)
        => Fail(errors);

    public static new Result<TValue> Failure(
        List<Error> errors,
        int statusCode)
        => Fail(errors, statusCode);

    public static new Result<TValue> Fail(
        params Error[] errors)
        => Fail(errors.ToList());

    public static new Result<TValue> Fail(
        List<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new Result<TValue>(
            isSuccess: false,
            value: default!,
            errors: errors.ToList());
    }

    public static new Result<TValue> Fail(
        List<Error> errors,
        int statusCode)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new Result<TValue>(
            isSuccess: false,
            value: default!,
            errors: errors.ToList(),
            statusCode: statusCode);
    }

    #endregion

    #endregion
}