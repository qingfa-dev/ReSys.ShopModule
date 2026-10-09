using BuildingBlock.Monad.Errors;
using BuildingBlock.Monad.Results;

namespace BuildingBlock.Monad.UnitTest.Results;

/// <summary>Tests for implicit conversion operators on <see cref="Result{TValue}"/>.</summary>
[Trait("Category", "Unit")]
public class ResultGenericImplicitSpec
{
    #region Success Conversion

    /// <summary>TValue should implicitly convert to generic success.</summary>
    [Theory]
    [InlineData("value")]
    [InlineData("")]
    [InlineData("another value")]
    public void TValue_Should_Implicitly_Convert_To_Generic_Success(string value)
    {
        // Act
        Result<string> result = value;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(value);
        result.StatusCode.ShouldBe(ResultConstant.StatusCode.Ok);
        result.Errors.ShouldBeEmpty();
    }

    /// <summary>Value types should implicitly convert to generic success.</summary>
    [Theory]
    [InlineData(42)]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValueType_Should_Implicitly_Convert_To_Generic_Success(int value)
    {
        // Act
        Result<int> result = value;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(value);
        result.StatusCode.ShouldBe(ResultConstant.StatusCode.Ok);
    }

    /// <summary>A null reference value should throw when converted to generic success.</summary>
    [Fact]
    public void NullValue_Should_Throw_When_Converted_To_Generic_Success()
    {
        // Arrange
        string value = null!;

        // Act
        Action act = () =>
        {
            Result<string> result = value;
            _ = result.Value;
        };

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    #endregion

    #region Failure Conversion

    /// <summary>Error should implicitly convert to generic failure.</summary>
    [Theory]
    [MemberData(nameof(SingleErrorFailureCases))]
    public void Error_Should_Implicitly_Convert_To_Generic_Failure(Error error, int expectedStatusCode)
    {
        // Act
        Result<string> result = error;

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldBe(new[] { error });
        result.StatusCode.ShouldBe(expectedStatusCode);
        result.Value.ShouldBeNull();
    }

    public static TheoryData<Error, int> SingleErrorFailureCases()
    {
        var data = new TheoryData<Error, int>();
        data.Add(ResultStub.NotFoundError(), ResultConstant.StatusCode.NotFound);
        data.Add(ResultStub.ConflictError(), ResultConstant.StatusCode.Conflict);
        return data;
    }

    /// <summary>Error array should implicitly convert to generic failure.</summary>
    [Theory]
    [MemberData(nameof(StringErrorArrayCases))]
    public void ErrorArray_Should_Implicitly_Convert_To_Generic_Failure(Error[] errors, int expectedStatusCode)
    {
        // Act
        Result<string> result = errors;

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldBe(errors);
        result.StatusCode.ShouldBe(expectedStatusCode);
    }

    public static TheoryData<Error[], int> StringErrorArrayCases()
    {
        var data = new TheoryData<Error[], int>();
        data.Add([ResultStub.NotFoundError()], ResultConstant.StatusCode.NotFound);
        data.Add([ResultStub.ConflictError()], ResultConstant.StatusCode.Conflict);
        return data;
    }

    /// <summary>Error list and multiple errors should implicitly convert to generic failure.</summary>
    [Theory]
    [MemberData(nameof(IntErrorCollectionCases))]
    public void ErrorList_And_MultipleErrors_Should_Implicitly_Convert_To_Generic_Failure(
        bool useList,
        Error[] errors,
        int expectedCount)
    {
        // Arrange
        List<Error> list = [.. errors];

        // Act
        Result<int> result;
        if (useList)
        {
            result = list;
        }
        else
        {
            result = errors;
        }

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(expectedCount);
        result.Errors.ShouldBe(errors);
        result.StatusCode.ShouldBe(ResultConstant.StatusCode.NotFound);
    }

    public static TheoryData<bool, Error[], int> IntErrorCollectionCases()
    {
        var data = new TheoryData<bool, Error[], int>();
        data.Add(true, [ResultStub.NotFoundError()], 1);
        data.Add(
            false,
            [ResultStub.NotFoundError(), Error.NotFound("test.other", "Another missing resource")],
            2);
        return data;
    }

    #endregion

    #region Failure Conversion Guards

    /// <summary>Invalid error collections should throw when converted.</summary>
    [Theory]
    [InlineData("empty-array")]
    [InlineData("empty-list")]
    [InlineData("mixed-array")]
    public void Invalid_Error_Collections_Should_Throw_When_Converted(string collectionKind)
    {
        // Arrange
        Error[] array = [];
        if (collectionKind == "mixed-array")
        {
            array = [ResultStub.NotFoundError(), ResultStub.ConflictError()];
        }

        List<Error> list = [];

        // Act
        Action act = () =>
        {
            if (collectionKind == "empty-list")
            {
                Result<int> result = list;
                _ = result.StatusCode;
            }
            else
            {
                Result<int> result = array;
                _ = result.StatusCode;
            }
        };

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    /// <summary>A null error list should throw when converted.</summary>
    [Fact]
    public void NullErrorList_Should_Throw_When_Converted()
    {
        // Arrange
        List<Error> errors = null!;

        // Act
        Action act = () =>
        {
            Result<int> result = errors;
            _ = result.StatusCode;
        };

        // Assert
        var ex = Should.Throw<ArgumentNullException>(act);
        ex.ParamName.ShouldBe("errors");
    }

    #endregion

    #region Direct Returns

    /// <summary>Direct returns should convert values and errors to generic results.</summary>
    [Fact]
    public void Direct_Returns_Should_Convert_Values_And_Errors_To_Generic_Results()
    {
        // Act
        var success = ReturnValue();
        var failure = ReturnError();
        var listFailure = ReturnErrorList();

        // Assert
        success.IsSuccess.ShouldBeTrue();
        success.Value.ShouldBe(42);
        failure.IsFailure.ShouldBeTrue();
        failure.Errors.ShouldContain(error => error.Code == "test.not_found");
        listFailure.IsFailure.ShouldBeTrue();
        listFailure.Errors.ShouldContain(error => error.Code == "test.not_found");

        static Result<int> ReturnValue() => 42;
        static Result<int> ReturnError() => ResultStub.NotFoundError();
        static Result<int> ReturnErrorList() => new List<Error> { ResultStub.NotFoundError() };
    }

    #endregion
}
