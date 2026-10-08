using BuildingBlock.Monad.Metadata;

namespace BuildingBlock.Monad.UnitTest.Metadata;

/// <summary>
/// Tests for <see cref="MetadataConstant"/> keyword constants.
/// </summary>
/// <remarks>
/// Verifies that metadata keywords remain stable and preserve their declared wire keys.
/// </remarks>
[Trait("Category", "Unit")]
public class MetadataConstantSpec
{
    #region Keyword constants

    [Theory]
    [InlineData(MetadataConstant.Keyword.Timestamp, "Timestamp")]
    [InlineData(MetadataConstant.Keyword.CorrelationId, "CorrelationId")]
    [InlineData(MetadataConstant.Keyword.RequestId, "RequestId")]
    [InlineData(MetadataConstant.Keyword.CausationId, "CausationId")]
    public void Keyword_Should_Preserve_Stable_Wire_Key(
        string keyword,
        string stableKey)
    {
        // Arrange

        // Act

        // Assert
        keyword.ShouldBe(stableKey);
    }

    #endregion
}
