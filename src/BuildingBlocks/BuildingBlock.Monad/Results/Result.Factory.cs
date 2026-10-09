using BuildingBlock.Monad.Errors;

namespace BuildingBlock.Monad.Results;

public partial record Result
{
    #region Factory

    #region Custom

    public static Result Custom(
        bool isSuccess,
        List<Error>? errors = null,
        int? statusCode = null)
        => new(
            isSuccess,
            errors?.ToList(),
            statusCode);

    #endregion

    #region Success

    public static Result Success(
        int statusCode = ResultConstant.StatusCode.Ok)
        => new(
            isSuccess: true,
            statusCode: statusCode);

    public static Result Ok()
        => Success(ResultConstant.StatusCode.Ok);

    public static Result Created()
        => Success(ResultConstant.StatusCode.Created);

    public static Result Accepted()
        => Success(ResultConstant.StatusCode.Accepted);

    public static Result NoContent()
        => Success(ResultConstant.StatusCode.NoContent);

    #endregion

    #region Failure

    public static Result Failure(
        List<Error> errors)
        => Fail(errors);

    public static Result Failure(
        List<Error> errors,
        int statusCode)
        => Fail(errors, statusCode);

    public static Result Fail(
        params Error[] errors)
        => Fail(errors.ToList());

    public static Result Fail(
        List<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new Result(
            isSuccess: false,
            errors: errors.ToList());
    }

    public static Result Fail(
        List<Error> errors,
        int statusCode)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new Result(
            isSuccess: false,
            errors: errors,
            statusCode: statusCode);
    }

    #endregion

    #endregion
}