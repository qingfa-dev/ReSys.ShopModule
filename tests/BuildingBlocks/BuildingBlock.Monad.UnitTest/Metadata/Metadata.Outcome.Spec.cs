using BuildingBlock.Monad.Metadata;

namespace BuildingBlock.Monad.UnitTest.Metadata;

/// <summary>
/// Tests for <see cref="MetadataOutcome"/> success and failure cases.
/// </summary>
/// <remarks>
/// Verifies that metadata outcome cases expose stable codes and meaningful messages,
/// and that failure codes follow the expected naming convention.
/// </remarks>
[Trait("Category", "Unit")]
public class MetadataOutcomeSpec
{
    #region Success

    [Fact]
    public void Success_Should_Be_Discoverable()
    {
        // Arrange
        var success = typeof(MetadataOutcome.Success);

        // Act

        // Assert
        success.ShouldNotBeNull();
    }

    #endregion

    #region Failure

    [Fact]
    public void Failure_Should_Expose_Expected_Cases()
    {
        // Arrange
        var failure = typeof(MetadataOutcome.Failure);

        // Act
        var cases = CollectOutcomeTypes(failure).ToArray();

        // Assert
        cases.ShouldNotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(FailureCases))]
    public void Failure_Case_Should_Define_Valid_Code_And_Message(
        string typeName,
        string code,
        string message)
    {
        // Arrange

        // Act

        // Assert
        typeName.ShouldNotBeNullOrWhiteSpace();
        code.ShouldNotBeNullOrWhiteSpace();
        message.ShouldNotBeNullOrWhiteSpace();

        code.ShouldMatch(
            @"^[a-z][a-z0-9_]*(\.[a-z][a-z0-9_]+)+$");
    }

    public static TheoryData<string, string, string> FailureCases()
    {
        var data = new TheoryData<string, string, string>();

        foreach (var type in CollectOutcomeTypes(typeof(MetadataOutcome.Failure)))
        {
            data.Add(
                type.FullName!,
                GetConstant(type, "Code"),
                GetConstant(type, "Message"));
        }

        return data;
    }

    #endregion

    #region Helpers

    private static IEnumerable<Type> CollectOutcomeTypes(Type type)
    {
        foreach (var nested in type.GetNestedTypes())
        {
            if (HasConstant(nested, "Code"))
            {
                yield return nested;
                continue;
            }

            foreach (var deeper in CollectOutcomeTypes(nested))
            {
                yield return deeper;
            }
        }
    }

    private static bool HasConstant(Type type, string name)
        => type.GetField(
            name,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Static) is not null;

    private static string GetConstant(Type type, string name)
        => (string)type.GetField(
            name,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Static)!
            .GetValue(null)!;

    #endregion
}
