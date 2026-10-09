using BuildingBlock.Monad.Errors;
using BuildingBlock.Monad.Results;

namespace BuildingBlock.Monad.UnitTest.Results;

/// <summary>Tests for implicit conversion operators on <see cref="Result"/>.</summary>
[Trait("Category", "Unit")]
public class ResultImplicitSpec
{
    #region Non-generic Failure

    /// <summary>Error and error array should implicitly convert to non-generic failure.</summary>
    [Theory]
    [MemberData(nameof(NonGenericFailureCases))]
    public void Errors_Should_Implicitly_Convert_To_NonGeneric_Failure(bool singleError, Error[] errors)
    {
        // Act
        Result result;
        if (singleError)
        {
            result = errors[0];
        }
        else
        {
            result = errors;
        }

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldBe(errors);
        result.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
    }

    public static TheoryData<bool, Error[]> NonGenericFailureCases()
    {
        var data = new TheoryData<bool, Error[]>();
        data.Add(true, [ResultStub.NotFoundError()]);
        data.Add(false, [ResultStub.NotFoundError()]);
        data.Add(false, [ResultStub.NotFoundError(), ResultStub.NotFoundError()]);
        return data;
    }

    #endregion

    #region Error Collection

    /// <summary>Error collections should use Fail factory for non-generic results.</summary>
    [Fact]
    public void Error_Collection_Should_Use_Fail_Factory_For_NonGeneric_Results()
    {
        // Arrange
        List<Error> errors = new List<Error> { ResultStub.NotFoundError() };

        // Act
        Result nonGeneric = Result.Fail(errors);

        // Assert
        nonGeneric.IsFailure.ShouldBeTrue();
        nonGeneric.Errors.ShouldBe(errors);
    }

    #endregion

    #region Implicit List Conversion

    /// <summary>Error lists should implicitly convert to non-generic failures.</summary>
    [Fact]
    public void Error_List_Should_Implicitly_Convert_To_NonGeneric_Failure()
    {
        // Arrange
        List<Error> errors = [ResultStub.NotFoundError()];

        // Act
        Result result = errors;

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldBe(errors);
        result.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
    }

    #endregion
}
