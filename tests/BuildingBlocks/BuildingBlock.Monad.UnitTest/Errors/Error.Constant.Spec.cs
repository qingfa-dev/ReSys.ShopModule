using System.Reflection;

using BuildingBlock.Monad.Errors;

namespace BuildingBlock.Monad.UnitTest.Errors;

/// <summary>Tests for <see cref="ErrorConstant"/> constraint limits, metadata keys, and failure rules.</summary>
/// <remarks>
/// Verifies that constraint values match documented limits, metadata keys round-trip as stable wire keys,
/// and every failure rule exposes a non-empty code and message.
/// </remarks>
[Trait("Category", "Unit")]
public class ErrorConstantSpec
{
    #region Constraints

    [Fact]
    public void Constraint_Should_Define_Documented_Limits()
    {
        // Arrange
        // Act
        // Assert
        ErrorConstant.Constraint.Code.MaxLength.ShouldBe(256);
        ErrorConstant.Constraint.Message.MaxLength.ShouldBe(1024);
        ErrorConstant.Constraint.Type.MaxLength.ShouldBe(256);
        ErrorConstant.Constraint.Status.Min.ShouldBe(100);
        ErrorConstant.Constraint.Status.Max.ShouldBe(599);
    }

    #endregion

    #region Metadata keys

    [Theory]
    [InlineData(ErrorConstant.Metadata.TraceId, "TraceId")]
    [InlineData(ErrorConstant.Metadata.Timestamp, "Timestamp")]
    [InlineData(ErrorConstant.Metadata.Resource, "Resource")]
    [InlineData(ErrorConstant.Metadata.Field, "Field")]
    [InlineData(ErrorConstant.Metadata.Attempt, "Attempt")]
    public void Metadata_Key_Should_Round_Trip_As_Stable_Wire_Key(string key, string stableKey)
    {
        // Arrange
        // Act
        // Assert
        key.ShouldBe(stableKey);
    }

    #endregion

    #region Failure rules

    [Fact]
    public void Failure_Rules_Should_Be_Discoverable()
    {
        // Arrange
        // Act
        var rules = CollectRuleTypes(typeof(ErrorConstant.Result.Failure));

        // Assert
        rules.ShouldNotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(FailureRules))]
    public void Failure_Rule_Should_Compose_Non_Empty_Code_And_Message(string typeName, string code, string message)
    {
        // Arrange
        // Act
        // Assert
        message.ShouldNotBeNullOrWhiteSpace();
        code.ShouldMatch(@"^[a-z][a-z0-9_]*(\.[a-z][a-z0-9_]+)+$");
        typeName.ShouldNotBeNullOrWhiteSpace();
    }

    public static TheoryData<string, string, string> FailureRules()
    {
        var data = new TheoryData<string, string, string>();

        foreach (var rule in CollectRuleTypes(typeof(ErrorConstant.Result.Failure)))
        {
            data.Add(
                rule.FullName!,
                GetConstant(rule, "Code"),
                GetConstant(rule, "Message"));
        }

        return data;
    }

    #endregion

    private static IEnumerable<Type> CollectRuleTypes(Type type)
    {
        foreach (var nested in type.GetNestedTypes())
        {
            if (nested.GetField("Code") is not null && nested.GetField("Message") is not null)
            {
                yield return nested;
            }

            foreach (var deeper in CollectRuleTypes(nested))
            {
                yield return deeper;
            }
        }
    }

    private static string GetConstant(Type type, string name)
        => (string)type.GetField(name, BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
}