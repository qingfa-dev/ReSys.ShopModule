using BuildingBlock.Monad.Errors;

namespace BuildingBlock.Monad.UnitTest.Errors;

/// <summary>Tests for <see cref="ErrorSeverity"/> enum values.</summary>
/// <remarks>
/// Verifies that severity levels have stable wire values for serialization and comparison.
/// </remarks>
[Trait("Category", "Unit")]
public class ErrorSeveritySpec
{
    #region Wire values

    [Theory]
    [InlineData(ErrorSeverity.Info, 0)]
    [InlineData(ErrorSeverity.Warning, 1)]
    [InlineData(ErrorSeverity.Error, 2)]
    [InlineData(ErrorSeverity.Critical, 3)]
    public void ErrorSeverity_Should_Have_Stable_Wire_Values(ErrorSeverity severity, int expected)
    {
        // Arrange
        // Act
        var actual = (int)severity;

        // Assert
        actual.ShouldBe(expected);
    }

    #endregion
}