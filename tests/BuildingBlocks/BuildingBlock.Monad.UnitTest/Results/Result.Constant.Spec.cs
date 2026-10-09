using BuildingBlock.Monad.Results;

namespace BuildingBlock.Monad.UnitTest.Results;

/// <summary>Tests for <see cref="ResultConstant"/> constraint and default values.</summary>
[Trait("Category", "Unit")]
public class ResultConstantSpec
{
    #region Constraints

    /// <summary>Constraints should expose status and collection limits.</summary>
    [Theory]
    [InlineData("Constraint.Status.Min", ResultConstant.Constraint.Status.Min, 100)]
    [InlineData("Constraint.Status.Max", ResultConstant.Constraint.Status.Max, 599)]
    [InlineData("Constraint.Status.Success.Min", ResultConstant.Constraint.Status.Success.Min, 200)]
    [InlineData("Constraint.Status.Success.Max", ResultConstant.Constraint.Status.Success.Max, 299)]
    [InlineData("Constraint.Status.Failure.Min", ResultConstant.Constraint.Status.Failure.Min, 400)]
    [InlineData("Constraint.Status.Failure.Max", ResultConstant.Constraint.Status.Failure.Max, 599)]
    [InlineData("Constraint.Errors.MaxCount", ResultConstant.Constraint.Errors.MaxCount, 50)]
    [InlineData("Constraint.Metadata.MaxEntries", ResultConstant.Constraint.Metadata.MaxEntries, 50)]
    public void Constraints_Should_Expose_Status_And_Collection_Limits(string name, int actual, int expected)
    {
        // Act & Assert
        actual.ShouldBe(expected, name);
    }

    #endregion

    #region Status Codes

    /// <summary>Status codes should match their HTTP values.</summary>
    [Theory]
    [InlineData(ResultConstant.StatusCode.Ok, 200)]
    [InlineData(ResultConstant.StatusCode.Created, 201)]
    [InlineData(ResultConstant.StatusCode.Accepted, 202)]
    [InlineData(ResultConstant.StatusCode.NoContent, 204)]
    [InlineData(ResultConstant.StatusCode.NotFound, 404)]
    [InlineData(ResultConstant.StatusCode.InternalServerError, 500)]
    public void Status_Codes_Should_Match_Their_Http_Values(int actual, int expected)
    {
        // Act & Assert
        actual.ShouldBe(expected);
    }

    #endregion

    #region Defaults

    /// <summary>Defaults should expose internal server error status and empty errors.</summary>
    [Fact]
    public void Default_Should_Expose_Internal_Server_Error_Status_And_Empty_Errors()
    {
        // Act & Assert
        ResultConstant.Default.FailureStatus.ShouldBe(500);
        ResultConstant.Default.EmptyErrors.ShouldBeEmpty();
    }

    #endregion

    #region Failure Constants

    /// <summary>Failure constants should provide stable codes and messages.</summary>
    [Fact]
    public void Failure_Constants_Should_Provide_Stable_Codes_And_Messages()
    {
        // Act & Assert
        ResultOutcome.Failure.Errors.MixedStatusCodes.Code.ShouldBe("result.errors.mixed_status_codes");
        ResultOutcome.Failure.Errors.MixedStatusCodes.Message.ShouldNotBeNullOrWhiteSpace();
        ResultOutcome.Failure.Value.Required.Code.ShouldBe("result.value.required");
        ResultOutcome.Failure.Status.InvalidFailureStatus(500).Message.ShouldContain("500");
    }

    /// <summary>Exception mapping missing should carry a stable code and message.</summary>
    [Fact]
    public void ExceptionMappingMissing_Should_Carry_A_Stable_Code_And_Message()
    {
        // Arrange
        var error = ResultOutcome.Failure.Flow.ExceptionMappingMissing(nameof(ResultExtension));

        // Act & Assert
        error.Code.ShouldBe("result.flow.exception_mapping_missing");
        error.Message.ShouldContain(nameof(ResultExtension));
    }

    #endregion
}